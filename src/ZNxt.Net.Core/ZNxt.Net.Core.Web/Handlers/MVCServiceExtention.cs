using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using System;
using System.IdentityModel.Tokens.Jwt;
using ZNxt.Net.Core.Config;
using ZNxt.Net.Core.Consts;
using ZNxt.Net.Core.DB.Mongo;
using ZNxt.Net.Core.Enums;
using ZNxt.Net.Core.Helpers;
using ZNxt.Net.Core.Interfaces;
using ZNxt.Net.Core.Services;
using ZNxt.Net.Core.Web.ContentHandler;
using ZNxt.Net.Core.Web.Handlers;
using ZNxt.Net.Core.Web.Helper;
using ZNxt.Net.Core.Web.Proxies;
using ZNxt.Net.Core.Web.Services;
using ZNxt.Net.Core.Web.Services.Api.Installer;

public static class MVCServiceExtention
{
    public static void AddZNxtApp(this IServiceCollection services)
    {
        // services.AddScoped<IDBServiceConfig>(new Func<IServiceProvider, IDBServiceConfig>(f => { return new MongoDBServiceConfig("DotNetCoreTest", "mongodb://localhost:27017"); }));
        services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);
        services.AddTransient<IServiceResolver, ServiceResolver>();
        services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
        services.AddSingleton<IEncryption, EncryptionService>();
        services.AddSingleton<UserAccontHelper, UserAccontHelper>();

        services.AddSingleton<IDBServiceConfig>(new MongoDBServiceConfig());
        services.AddSingleton<IKeyValueStorage, FileKeyValueFileStorage>();
        services.AddTransient<IDBService, MongoDBService>();
        services.AddSingleton<IRouting, Routing>();
        services.AddTransient<IAssemblyLoader, AssemblyLoader>();
        services.AddTransient<IStaticContentHandler, ZNxt.Net.Core.Web.ContentHandler.StaticContentHandler>();
        services.AddTransient<ILogger, Logger>();
        services.AddTransient<ILogReader, Logger>();
        services.AddTransient<IHttpContextProxy, HttpContextProxy>();
        services.AddTransient<IHttpFileUploader, HttpContextProxy>();
        services.AddTransient<IViewEngine, RazorTemplateEngine>();
        services.AddTransient<IActionExecuter, ActionExecuter>();
        services.AddTransient<IResponseBuilder, ResponseBuilder>();
        services.AddTransient<IAppSettingService, AppSettingService>();
        services.AddTransient<ISessionProvider, SessionProvider>();
        services.AddTransient<IApiGatewayService, ApiGatewayService>();
        services.AddTransient<IInMemoryCacheService, InMemoryCacheService>();
        services.AddMemoryCache();
        var serviceProvider = services.BuildServiceProvider();
        SetAppInstallStatus(serviceProvider);
        InitRoutingDepedencies(services, serviceProvider);
    }

    public static void AddZNxtIdentityServer(this IServiceCollection services)
    {
        var ssourl = CommonUtility.GetAppConfigValue(CommonConst.CommonValue.SSOURL_CONFIG_KEY);
        var appName = CommonUtility.GetAppConfigValue(CommonConst.CommonValue.APP_NAME_CONFIG_KEY);
        var appSecret = CommonUtility.GetAppConfigValue(CommonConst.CommonValue.APP_SECRET_CONFIG_KEY);
        
        services.AddAuthentication(options =>
        {
            options.DefaultChallengeScheme = "oidc";
            options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            options.DefaultAuthenticateScheme = "oidc";

        }).AddOpenIdConnect("oidc", options =>
        {
            options.Authority = ssourl;
            options.ClientId = appName;
            options.ClientSecret = appSecret;
            options.ResponseType = OpenIdConnectResponseType.CodeIdToken;
            options.Scope.Add("openid");
            options.Scope.Add("profile");
            options.Scope.Add("ZNxtCoreAppApi");
            options.SignedOutRedirectUri = "/";
            options.TokenValidationParameters.NameClaimType = "name";
            options.SaveTokens = true;
            options.GetClaimsFromUserInfoEndpoint = true;
            options.RequireHttpsMetadata = false;
        })
            .AddCookie();
    }

    public static void AddZNxtBearerAuthentication(this IServiceCollection services)
    {
        var ssourl = CommonUtility.GetAppConfigValue(CommonConst.CommonValue.SSOURL_CONFIG_KEY);
        JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();
        services.AddAuthentication().AddJwtBearer(options =>
        {

            options.Authority = ssourl;
            options.Audience = "ZNxtCoreAppApi";
            options.TokenValidationParameters.NameClaimType = "name";
            options.RequireHttpsMetadata = false;
        });
    }

    private static void SetAppInstallStatus(ServiceProvider serviceProvider)
    {
        try
        {
            ApplicationConfig.AppInstallStatus = AppInstallStatus.Finish;
           
            // Skip installation process.

            //var db = serviceProvider.GetService<IDBService>();
            //if (db.IsConnected)
            //{
            //    var appSetting = serviceProvider.GetService<IAppSettingService>();
            //    ApplicationConfig.AppInstallStatus = AppInstallStatus.Init;
            //    var appstatus = appSetting.GetAppSettingData(CommonConst.CommonValue.APPINSTALLSTATUS);
            //    Enum.TryParse(typeof(AppInstallStatus), appstatus, true, out object status);
            //    if (status != null)
            //    {
            //        ApplicationConfig.AppInstallStatus = (AppInstallStatus)status;
            //    }
            //    if (ApplicationConfig.AppInstallStatus != AppInstallStatus.Finish)
            //    {
            //        CommonConst.CommonValue.APP_FRONTEND_FOLDERPATH = "appinstall";
            //    }
            //}
            //else
            //{
            //    ApplicationConfig.AppInstallStatus = ZNxt.Net.Core.Enums.AppInstallStatus.DBNotSet;
            //    CommonConst.CommonValue.APP_FRONTEND_FOLDERPATH = "appinstall";
            //}
        }
        catch (Exception)
        {
            throw;
        }
    }

    private static void InitRoutingDepedencies(IServiceCollection services, ServiceProvider serviceProvider)
    {
        var routing = serviceProvider.GetService<IRouting>();
        var assemblyLoader = serviceProvider.GetService<IAssemblyLoader>();
        foreach (var route in routing.GetRoutes())
        {
            var type = assemblyLoader.GetType(route.ExecultAssembly, route.ExecuteType);
            if (type != null)
            {
                // Do Not add AppInstallerContoller controller if AppInstallStatus is Finish for security reason 
                if (!(ApplicationConfig.AppInstallStatus == AppInstallStatus.Finish && type == typeof(AppInstallerContoller)))
                {
                    services.AddTransient(type, type);
                }
            }
        }
    }
}

public static class MVCApplicationBuilderExtensions
{
    public static void UseZNxtApp(this IApplicationBuilder app)
    {
        app.UseHttpProxyHandler();
        app.Map("/api", HandlerAPI);
        app.UseMvcWithDefaultRoute();
        app.MapWhen(context => true, HandlerStaticContant);
    }
    private static void HandlerStaticContant(IApplicationBuilder app)
    {
        app.UseStaticContentHandler();

    }
    private static void HandlerAPI(IApplicationBuilder app)
    {
        app.UseApiHandler();

    }
}

