using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NLog;
using NLog.Web;
using System;

namespace BulkBatchApp
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var nlogEnvironment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
            if (nlogEnvironment == "Production")
            {
                LogManager.LoadConfiguration("NLog.config");
            }
            else
            {
                LogManager.LoadConfiguration("NLog.Development.config");
            }

            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args)
        {
            return Host.CreateDefaultBuilder(args)
.ConfigureWebHostDefaults(webBuilder =>
{
webBuilder.UseStartup<Startup>()
.ConfigureLogging(logging =>
{
logging.ClearProviders();
}).UseNLog();
});
        }
    }
}
