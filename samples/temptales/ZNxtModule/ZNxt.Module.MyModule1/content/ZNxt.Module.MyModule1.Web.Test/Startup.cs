﻿using Microsoft.AspNetCore.Builder;
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
using ZNxt.Module.MyModule1.Consts;

namespace ZNxt.Module.MyModule1.Web.Test
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
            LoadModules();
            services.AddCors();
            services.AddMvc();
            services.AddZNxtSSO(Environment);
            services.AddZNxtApp();
            services.AddZNxtBearerAuthentication();
            services.AddZNxtIdentityServer();
            services.AddTransient<ILogger, ConsoleLogger>();
            services.AddTransient<ILogReader, ConsoleLogger>();
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
            app.UseZNxtSSO();
            var ssourl = CommonUtility.GetAppConfigValue(CommonConst.CommonValue.SSOURL_CONFIG_KEY);
            if (!string.IsNullOrEmpty(ssourl))
            {
                app.UseAuthentication();
            }
            app.UseZNxtApp();

        }

        public static void LoadModules()
        {
            Console.WriteLine("Loading modules .... ");
            Console.WriteLine(MyModule1Consts.GetServiceInfo());
        }
    }
}
