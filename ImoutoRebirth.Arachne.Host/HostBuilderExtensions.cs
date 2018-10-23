using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace ImoutoRebirth.Arachne.Host
{
    public static class HostBuilderExtensions
    {
        public static IHostBuilder UseStartup(this IHostBuilder builder)
        {
            builder.ConfigureServices((context, collection) =>
            {
                var startup = new Startup(context.Configuration);
                startup.ConfigureServices(collection);
            });

            return builder;
        }

        public static IHostBuilder UseConfiguration(this IHostBuilder hostBuilder)
        {
            hostBuilder.ConfigureAppConfiguration((context, builder) =>
                builder.AddJsonFile("appSettings.json", false, true)
                       .AddEnvironmentVariables("ARACHNE_"));

            return hostBuilder;
        }
    }
}