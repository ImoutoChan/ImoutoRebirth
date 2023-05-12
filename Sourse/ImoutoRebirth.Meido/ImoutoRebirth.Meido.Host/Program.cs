using ImoutoRebirth.Common.EntityFrameworkCore;
using ImoutoRebirth.Common.Host;
using ImoutoRebirth.Common.Logging;
using ImoutoRebirth.Common.Quartz.Extensions;
using ImoutoRebirth.Meido.DataAccess;
using Microsoft.Extensions.Hosting;

namespace ImoutoRebirth.Meido.Host;

internal class Program
{
    private const string ServicePrefix = "MEIDO_";

    private static async Task Main(string[] args)
    {
        await CreateConsoleHost(args)
            .MigrateIfNecessary<MeidoDbContext>()
            .RunAsync();
    }

    public static IHost CreateConsoleHost(string[] args)
        => new HostBuilder()
            .UseWindowsService()
            .SetWorkingDirectory()
            .UseEnvironmentFromEnvironmentVariable(ServicePrefix)
            .UseConfiguration(ServicePrefix)
            .ConfigureSerilog(
                (loggerBuilder, appConfiguration) 
                    => loggerBuilder
                        .WithoutDefaultLoggers()
                        .WithConsole()
                        .WithAllRollingFile()
                        .WithInformationRollingFile()
                        .PatchWithConfiguration(appConfiguration))
            .UseQuartz()
            .UseStartup(x => new Startup(x))
            .Build();

    /// <summary>
    /// Hack for EFCore DesignTime DbContext construction.
    /// </summary>
    public static IHost BuildWebHost(string[] args) => CreateConsoleHost(args);
}