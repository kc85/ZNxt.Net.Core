using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Https;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using ZNxt.Net.Core.Config;

namespace ZNxt.Net.Core.Web.Sample
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
          Host.CreateDefaultBuilder(args)
              .ConfigureWebHostDefaults(webBuilder =>
              {
                  webBuilder.UseStartup<Startup>()
                  .ConfigureKestrel(options =>
                  {
                      var fileName = "ZNxtIdentitySigning.pfx";
                      var cert = new X509Certificate2(fileName, "abc@123");
                      options.Listen(IPAddress.Any, ApplicationConfig.HttpPort);
                      options.Listen(IPAddress.Any, ApplicationConfig.HttpsPort,o =>
                      {
                          o.UseHttps(cert);
                       
                      });
                  });
              });

        //public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>

        //    WebHost.CreateDefaultBuilder(args)
        //      .UseKestrel(options =>
        //      {
        //          var fileName = "ZNxtIdentitySigning.pfx";
        //          var cert = new X509Certificate2(fileName, "abc@123");

        //          options.Listen(IPAddress.Any, ApplicationConfig.HttpPort);
        //          options.Listen(IPAddress.Any, ApplicationConfig.HttpsPort, listenOptions =>
        //          {
        //              listenOptions.UseHttps(cert);
        //          });
        //      })
        //      .UseStartup<Startup>();

    }
}
