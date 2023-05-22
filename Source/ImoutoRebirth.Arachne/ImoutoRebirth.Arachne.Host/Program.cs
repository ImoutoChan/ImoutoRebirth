using ImoutoRebirth.Common.Host;
using ImoutoRebirth.Common.Logging;
using Microsoft.Extensions.Hosting;

namespace ImoutoRebirth.Arachne.Host;

internal class Program
{
    private const string ServicePrefix = "ARACHNE_";

    private static async Task Main(string[] args)
    {
        await CreateConsoleHost(args)
            .RunAsync();
    }

    public static IHost CreateConsoleHost(string[] args)
        => new HostBuilder()
            .SetWorkingDirectory()
            .UseWindowsService()
            .UseEnvironmentFromEnvironmentVariable(ServicePrefix)
            .UseConfiguration(ServicePrefix)
            .ConfigureSerilog(
                (loggerBuilder, appConfiguration, hostEnvironment)
                    => loggerBuilder
                        .WithoutDefaultLoggers()
                        .WithConsole()
                        .WithAllRollingFile()
                        .WithInformationRollingFile()
                        .WithOpenSearch(appConfiguration, hostEnvironment))
            .UseStartup(x => new Startup(x))
            .Build();
}
