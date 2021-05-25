using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.HttpOverrides;
using ZNxt.Net.Core.Helpers;
using ZNxt.Net.Core.Consts;
using System;
using Microsoft.Extensions.Hosting;
using ZNxt.Net.Core.Interfaces;
using ZNxt.Net.Core.Web.Services;
using ZNxt.Identity.Services;
using ZNxt.Net.Core.Web.Interfaces;
using ZNxt.Module.Identity.MySql.Services.API;
using ZNxt.Net.Core.Web.Handlers;


namespace ZNxt.Net.Core.Web.SSOSample
{
    public class Startup
    {
        public IWebHostEnvironment Environment { get; }
        public IConfiguration Configuration { get; }
        public Startup(IWebHostEnvironment environment, IConfiguration configuration)
        {
            Environment = environment;
            Configuration = configuration;
        }
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddCors();

            services.AddMvc();
            services.AddZNxtSSO(Environment);
            services.AddZNxtApp();
            var serviceProvider = services.BuildServiceProvider();
            var routing = serviceProvider.GetService<IRouting>();
            LoadModules(routing);
            MVCServiceExtention.InitRoutingDepedencies(services, serviceProvider);
            services.AddTransient<IZNxtUserService, ZNxtUserRDBService>();
            services.AddTransient<IAppAuthTokenHandler, AppAuthTokenHandler>();
           // services.AddTransient<ITenantSetterService, TenantService>();
            services.AddZNxtBearerAuthentication();
            services.AddZNxtIdentityServer();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            var fordwardedHeaderOptions = new ForwardedHeadersOptions
            {
                ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
            };
            fordwardedHeaderOptions.KnownNetworks.Clear();
            fordwardedHeaderOptions.KnownProxies.Clear();

            app.UseForwardedHeaders(fordwardedHeaderOptions);
            var corurl = CommonUtility.GetAppConfigValue("cor_urls");
            if (string.IsNullOrEmpty(corurl))
            {
                corurl = "http://localhost:50071";
            }
            app.UseCors(
                    options => options.WithOrigins(corurl.Split(';')).AllowAnyMethod()
             );
            //app.UseZNxtSSO();
            //app.UseZNxtApp();

            app.UseIdentityServer();

            app.UseHttpProxyHandler();
            app.Map("/api", HandlerAPI);

            app.UseRouting();
            var ssourl = CommonUtility.GetAppConfigValue(CommonConst.CommonValue.SSOURL_CONFIG_KEY);
            if (!string.IsNullOrEmpty(ssourl))
            {
                app.UseAuthorization();
                app.UseAuthentication();
               
            }
            app.UseEndpoints(endpoints => endpoints.MapControllerRoute(
            name: "default",
            pattern: "{controller}/{action}/{id?}"));


            app.MapWhen(context => true, HandlerStaticContant);

        }

        public static void LoadModules(IRouting routing)
        {
            new ClientController(null, null, null, null, null, null);
            new ZNxtUserRDBService(null, null, null, null, null, null);
            routing.PushAssemblyRoute(typeof(ClientController).Assembly);
            Console.WriteLine("Loading modules .... ");
        }

        private static void HandlerAPI(IApplicationBuilder app)
        {
            app.UseApiHandler();

        }
        private static void HandlerStaticContant(IApplicationBuilder app)
        {
            app.UseStaticContentHandler();

        }

        //public static class MVCApplicationBuilderExtensions
        //{
        //    public static void UseZNxtApp(this IApplicationBuilder app)
        //    {
        //        app.UseHttpProxyHandler();
        //        app.Map("/api", HandlerAPI);
        //        //var useSpa = CommonUtility.GetAppConfigValue("UseSpa");

        //        //if (useSpa != null && useSpa.ToLower() == "true")
        //        //{
        //        //  //  app.UseStaticFiles();
        //        //   // app.UseSpaStaticFiles();
        //        //}


        //        //if (useSpa ==null || useSpa.ToLower() != "true")
        //        //{
        //        app.UseRouting();
        //        var ssourl = CommonUtility.GetAppConfigValue(CommonConst.CommonValue.SSOURL_CONFIG_KEY);
        //        if (!string.IsNullOrEmpty(ssourl))
        //        {
        //            app.UseAuthentication();
        //        }
        //        app.UseEndpoints(endpoints => endpoints.MapControllerRoute(
        //        name: "default",
        //        pattern: "{controller}/{action}/{id?}"));


        //        app.MapWhen(context => true, HandlerStaticContant);

        //        //  }
        //    }
        //    private static void HandlerStaticContant(IApplicationBuilder app)
        //    {
        //        app.UseStaticContentHandler();

        //    }
        //    private static void HandlerAPI(IApplicationBuilder app)
        //    {
        //        app.UseApiHandler();

        //    }
        //    public static void UseZNxtSSO(this IApplicationBuilder app)
        //    {

        //        if (!ApplicationConfig.IsSSO)
        //            return;
        //        app.UseIdentityServer();
        //        AccountOptions.ShowLogoutPrompt = false;
        //        AccountOptions.AutomaticRedirectAfterSignOut = true;
        //    }


        //}

    }
}
