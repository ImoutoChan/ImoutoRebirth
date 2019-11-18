using ImoutoRebirth.Common.EntityFrameworkCore;
using ImoutoRebirth.Common.Logging;
using ImoutoRebirth.Room.Database;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using System.Threading.Tasks;
using ImoutoRebirth.Common.Quartz.Extensions;
using ImoutoRebirth.Common.Host;

namespace ImoutoRebirth.Room.Webhost
{
    public class Program
    {
        private const string ServicePrefix = "ROOM_";

        public static async Task Main(string[] args)
        {
            await CreateHostBuilder(args)
               .Build()
               .MigrateIfNecessary<RoomDbContext>()
               .RunAsync();
        }

        public static IHostBuilder CreateHostBuilder(string[] args)
            => Host.CreateDefaultBuilder(args)
                   .UseWindowsService()
                   .UseQuartz()
                   .UseConfiguration(ServicePrefix)
                   .ConfigureSerilog(
                        (loggerBuilder, appConfiguration) 
                            => loggerBuilder
                                .WithoutDefaultLoggers()
                                .WithConsole()
                                .WithAllRollingFile()
                                .WithInformationRollingFile()
                                .PatchWithConfiguration(appConfiguration))
                   .ConfigureWebHostDefaults(builder => builder.UseStartup<Startup>());
    }
}
