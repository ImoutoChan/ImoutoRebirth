using ImoutoRebirth.Common.EntityFrameworkCore;
using ImoutoRebirth.Common.Logging;
using ImoutoRebirth.Room.Database;
using ImoutoRebirth.Common.Quartz.Extensions;
using ImoutoRebirth.Common.Host;
using ImoutoRebirth.Common.OpenTelemetry;

namespace ImoutoRebirth.Room.Webhost;

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
            .SetWorkingDirectory()
            .UseQuartz()
            .UseConfiguration(ServicePrefix)
            .ConfigureSerilog(
                (loggerBuilder, appConfiguration, hostEnvironment) 
                    => loggerBuilder
                        .WithoutDefaultLoggers()
                        .WithConsole()
                        .WithAllRollingFile()
                        .WithInformationRollingFile()
                        .WithOpenSearch(appConfiguration, hostEnvironment))
            .ConfigureWebHostDefaults(builder => builder.UseStartup<Startup>())
            .ConfigureServices((context, services) =>
            {
                services.AddOpenTelemetry(context.HostingEnvironment, context.Configuration);
            });
}
