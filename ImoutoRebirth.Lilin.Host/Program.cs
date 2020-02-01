using System.Threading.Tasks;
using ImoutoRebirth.Common.EntityFrameworkCore;
using ImoutoRebirth.Common.Host;
using ImoutoRebirth.Common.Logging;
using ImoutoRebirth.Common.Quartz.Extensions;
using ImoutoRebirth.Lilin.DataAccess;
using ImoutoRebirth.Lilin.WebApi;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using GenericHost = Microsoft.Extensions.Hosting.Host;

namespace ImoutoRebirth.Lilin.Host
{
    internal static class Program
    {
        private const string ServicePrefix = "LILIN_";

        private static async Task Main(string[] args)
        {
            await CreateHostBuilder(args)
                .Build()
                .MigrateIfNecessary<LilinDbContext>()
                .RunAsync();
        }

        // ReSharper disable once MemberCanBePrivate.Global
        public static IHostBuilder CreateHostBuilder(string[] args)
            => GenericHost.CreateDefaultBuilder(args)
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
                .UseStartup(x => new Startup(x))
                .UseQuartz()
                .ConfigureWebHostDefaults(
                    webHostBuilder
                        => webHostBuilder
                            .UseKestrel(options => options.AddServerHeader = false)
                            .UseStartup<WebApiStartup>());
    }
}
