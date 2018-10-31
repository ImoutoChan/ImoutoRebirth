using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace ImoutoRebirth.Common.Host
{
    public static class HostBuilderExtensions
    {
        public static IHostBuilder UseStartup(this IHostBuilder builder, Func<IConfiguration, BaseStartup> startupFabric)
        {
            ArgumentValidator.NotNull(() => startupFabric);

            builder.ConfigureServices((context, collection) =>
            {
                var startup = startupFabric.Invoke(context.Configuration);
                startup.ConfigureServices(collection);
            });

            return builder;
        }

        public static IHostBuilder UseEnvironmentFromEnvironmentVariable(
            this IHostBuilder builder,
            string servicePrefix)
        {
            var environment = Environment.GetEnvironmentVariable($"{servicePrefix}ENVIRONMENT");

            builder.UseEnvironment(environment ?? "Production");

            return builder;
        }

        public static IHostBuilder UseConfiguration(this IHostBuilder hostBuilder, string servicePrefix)
        {
            hostBuilder.ConfigureAppConfiguration(
                (context, builder) => builder.AddJsonFile("appSettings.json", false, true)
                                             .AddJsonFile(
                                                  $"appSettings.{context.HostingEnvironment.EnvironmentName}.json",
                                                  true,
                                                  true)
                                             .AddEnvironmentVariables(servicePrefix));

            return hostBuilder;
        }
    }
}