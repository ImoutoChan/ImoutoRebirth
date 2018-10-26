using System;
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

        public static IHostBuilder UseEnvironmentFromEnvironmentVariable(this IHostBuilder builder)
        {
            var environment = Environment.GetEnvironmentVariable("ARACHNE_ENVIRONMENT");

            builder.UseEnvironment(environment ?? "Production");
            
            return builder;
        }

        public static IHostBuilder UseConfiguration(this IHostBuilder hostBuilder)
        {
            hostBuilder.ConfigureAppConfiguration((context, builder) =>
                builder.AddJsonFile("appSettings.json", false, true)
                       .AddJsonFile($"appSettings.{context.HostingEnvironment.EnvironmentName}.json", true, true)
                       .AddEnvironmentVariables("ARACHNE_"));

            return hostBuilder;
        }
    }
}