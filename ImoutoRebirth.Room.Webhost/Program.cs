using System.Threading.Tasks;
using ImoutoRebirth.Common.EntityFrameworkCore;
using ImoutoRebirth.Common.Logging;
using ImoutoRebirth.Room.Database;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

namespace ImoutoRebirth.Room.Webhost
{
    public class Program
    {
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
                   .ConfigureSerilog(
                        (loggerBuilder, appConfiguration) => loggerBuilder
                                                            .WithoutDefaultLoggers()
                                                            .WithConsole()
                                                            .WithAllRollingFile()
                                                            .WithInformationRollingFile()
                                                            .PatchWithConfiguration(appConfiguration))
                   .ConfigureWebHostDefaults(builder => builder.UseStartup<Startup>());
    }
}
