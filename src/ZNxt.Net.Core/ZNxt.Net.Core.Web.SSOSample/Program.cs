using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using ZNxt.Net.Core.Config;

namespace ZNxt.Net.Core.Web.SSOSample
{
    public class Program
    {
        public static void Main(string[] args)
        {
            try
            {
                CreateWebHostBuilder(args).Build().Run();
            }
            catch (SocketException)
            {
                Console.WriteLine();
                var color = Console.ForegroundColor;

                var errmsg = "[Error]:: Please change your port  number on znxtsettings.json.config file, HttpPort and HttpsPort";
                Enumerable.Range(1, 10).All(x => {
                    Console.Write("*");
                    return true;
                });
                Console.WriteLine();
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(errmsg);
                Console.ForegroundColor = color;
                Enumerable.Range(1, 10).All(x => {
                    Console.Write("*");
                    return true;
                });
                Console.WriteLine();
            }

        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>

             WebHost.CreateDefaultBuilder(args)
               .UseKestrel(options =>
               {
                   var fileName = "ZNxtIdentitySigning.pfx";
                   var cert = new X509Certificate2(fileName, "abc@123");

                   options.Listen(IPAddress.Any, ApplicationConfig.HttpPort);
                   options.Listen(IPAddress.Any, ApplicationConfig.HttpsPort, listenOptions =>
                   {
                       listenOptions.UseHttps(cert);
                       listenOptions.Protocols = Microsoft.AspNetCore.Server.Kestrel.Core.HttpProtocols.Http1;
                   });
               })
               .UseStartup<Startup>();
    }
}
