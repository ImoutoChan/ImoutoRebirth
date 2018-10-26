using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;

namespace ImoutoRebirth.Arachne.Host
{
    internal class Program
    {
        private static async Task Main(string[] args)
        {
            await CreateConsoleHost(args).RunAsync();
        }

        public static IHost CreateConsoleHost(string[] args)
            => new HostBuilder()
              .UseConsoleLifetime()
              .UseEnvironmentFromEnvironmentVariable()
              .UseConfiguration()
              .UseStartup()
              .Build();
    }
}
