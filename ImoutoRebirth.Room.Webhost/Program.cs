using System.Threading.Tasks;
using ImoutoRebirth.Common.EntityFrameworkCore;
using ImoutoRebirth.Room.Database;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Core;

namespace ImoutoRebirth.Room.Webhost
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            await CreateWebHostBuilder(args)
               .Build()
               .MigrateIfNecessary<RoomDbContext>()
               .RunAsync();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                   .ConfigureLogging(
                        (context, builder) =>
                        {
                            builder.ClearProviders();
                            builder.AddSerilog(dispose: true, logger: GetSerilogLogger(context.Configuration));
                        })
                   .UseStartup<Startup>();

        private static Logger GetSerilogLogger(IConfiguration configuration)
            => new LoggerConfiguration()
              .ReadFrom.Configuration(configuration)
              .CreateLogger();
    }
}
