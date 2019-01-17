using System.Threading.Tasks;
using ImoutoRebirth.Common.EntityFrameworkCore;
using ImoutoRebirth.Common.Host;
using ImoutoRebirth.Lilin.DataAccess;
using Microsoft.Extensions.Hosting;

namespace ImoutoRebirth.Lilin.Host
{
    internal class Program
    {
        private const string ServicePrefix = "LILIN_";

        private static async Task Main(string[] args)
        {
            await CreateConsoleHost(args)
                 .MigrateIfNecessary<LilinDbContext>()
                 .RunAsync();
        }

        public static IHost CreateConsoleHost(string[] args)
            => new HostBuilder()
              .UseConsoleLifetime()
              .UseEnvironmentFromEnvironmentVariable(ServicePrefix)
              .UseConfiguration(ServicePrefix)
              .UseStartup(x => new Startup(x))
              .Build();

        /// <summary>
        /// Hack for EFCore DesignTime DbContext construction.
        /// </summary>
        public static IHost BuildWebHost(string[] args) => CreateConsoleHost(args);
    }
}
