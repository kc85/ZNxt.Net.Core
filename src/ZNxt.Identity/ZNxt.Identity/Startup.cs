// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using System;
using IdentityServer4;
using IdentityServer4.Quickstart.UI;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using System.Security.Cryptography.X509Certificates;
using System.IO;
using ZNxt.Identity.Services;
using IdentityServer4.Services;

namespace ZNxt.Identity
{
    public class Startup
    {
        public IHostingEnvironment Environment { get; }
        public IConfiguration Configuration { get; }

        public Startup(IHostingEnvironment environment, IConfiguration configuration)
        {
            Environment = environment;
            Configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc().SetCompatibilityVersion(Microsoft.AspNetCore.Mvc.CompatibilityVersion.Version_2_1);
            AddZNxtSSO(services);
            services.AddZNxtApp();
            services.AddZNxtBearerAuthentication();
        }

        private void AddZNxtSSO(IServiceCollection services)
        {
            services.Configure<IISOptions>(options =>
            {
                options.AutomaticAuthentication = false;
                options.AuthenticationDisplayName = "Windows";
            });

            var builder = services.AddIdentityServer(options =>
            {
                options.Events.RaiseErrorEvents = true;
                options.Events.RaiseInformationEvents = true;
                options.Events.RaiseFailureEvents = true;
                options.Events.RaiseSuccessEvents = true;
            })
            .AddTestUsers(TestUsers.Users);

            // in-memory, code config
            builder.AddInMemoryIdentityResources(Config.GetIdentityResources());
            builder.AddInMemoryApiResources(Config.GetApis());
            builder.AddInMemoryClients(Config.GetClients());
            if (Environment.IsDevelopment())
            {
                builder.AddDeveloperSigningCredential();
            }
            else
            {
                var fileName = Path.Combine(Environment.ContentRootPath, "ZNxtIdentitySigning.pfx");
                var cert = new X509Certificate2(fileName, "abc@123");
                builder.AddSigningCredential(cert);
            }
            services.AddAuthentication()
                .AddGoogle(options =>
                {
                    options.SignInScheme = IdentityServerConstants.ExternalCookieAuthenticationScheme;
                    options.ClientId = "592081696184-3056k8j98cfliger0398q08nmi50cfjs.apps.googleusercontent.com";
                    options.ClientSecret = "l-vFpRQvyZP_otetPhrF5Xdy";
                });
            services.AddTransient<IZNxtUserService, ZNxtUserService>();
            services.AddTransient<IProfileService, ZNxtProfileService>();
            services.AddTransient<ZNxtUserStore>();
            services.AddTransient<IUserNotifierService, UserNotifierService>();
        }

        public void Configure(IApplicationBuilder app)
        {
            if (Environment.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            UseZNxtSSO(app);
            app.UseStaticFiles();
            app.UseMvcWithDefaultRoute();
            app.UseZNxtApp();
        }

        private void UseZNxtSSO(IApplicationBuilder app)
        {
            app.UseIdentityServer();
            AccountOptions.ShowLogoutPrompt = false;
            AccountOptions.AutomaticRedirectAfterSignOut = true;
        }
    }
}