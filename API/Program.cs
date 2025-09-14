using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NLog.Web;

namespace API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            //CreateWebHostBuilder(args).Build().Run();
            // NLog: setup the logger first to catch all errors
            var logger = NLog.Web.NLogBuilder.ConfigureNLog("nlog.config").GetCurrentClassLogger();
            try
            {
                logger.Debug("init main");
                CreateWebHostBuilder(args).Build().Run();
            }
            catch (Exception ex)
            {
                //NLog: catch setup errors
                logger.Error(ex, "Stopped program because of exception");
                throw;
            }
            finally
            {
                // Ensure to flush and stop internal timers/threads before application-exit (Avoid segmentation fault on Linux)
                NLog.LogManager.Shutdown();
            }

        }

        //public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
        //    WebHost.CreateDefaultBuilder(args)
        //    //.UseKestrel(options =>
        //    //{
        //    //    options.Listen(IPAddress.Loopback, 5080);
        //    //})
        //    .UseStartup<Startup>();

        public static IWebHostBuilder CreateWebHostBuilder(string[] args)
        {
            var config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false)
                .Build();

            var port = config.GetValue<int>("ServerPort", 7998);
            var useHttps = config.GetValue<bool>("UseHttps", false);
            var protocol = useHttps ? "https" : "http";

            var builder = WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>()
                .UseUrls($"{protocol}://*:{port}")
                .ConfigureLogging(logging =>
                {
                    logging.ClearProviders();
                    logging.SetMinimumLevel(Microsoft.Extensions.Logging.LogLevel.Trace);
                })
                .UseNLog();  // NLog: setup NLog for Dependency injection

            // Configure HTTPS with certificate if enabled
            if (useHttps)
            {
                var certPath = config.GetValue<string>("Https:Certificate:Path");
                var certPassword = config.GetValue<string>("Https:Certificate:Password");
                var httpsPort = config.GetValue<int>("Https:Port", port);
                
                if (!string.IsNullOrEmpty(certPath))
                {
                    builder.UseKestrel(options =>
                    {
                        options.Listen(System.Net.IPAddress.Any, httpsPort, listenOptions =>
                        {
                            listenOptions.UseHttps(certPath, certPassword);
                        });
                    });
                }
            }

            return builder;
        }
    }
}
