using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.HttpOverrides;
using ZNxt.Net.Core.Helpers;
using ZNxt.Net.Core.Consts;
using Microsoft.Extensions.Hosting;


namespace ZNxt.Net.Core.Web.Sample
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
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddCors();
            services.AddMvc();//.SetCompatibilityVersion(Microsoft.AspNetCore.Mvc.CompatibilityVersion.Version_2_1);
            services.AddZNxtSSO(Environment);
            services.AddZNxtApp();
            services.AddZNxtBearerAuthentication();
            services.AddZNxtIdentityServer();
           
        }
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
            app.UseZNxtSSO();
            var ssourl = CommonUtility.GetAppConfigValue(CommonConst.CommonValue.SSOURL_CONFIG_KEY);
            if (!string.IsNullOrEmpty(ssourl))
            {
                app.UseAuthentication();
            }
            app.UseZNxtApp();
        }
    }

    

}
