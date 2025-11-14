using System.Reflection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace ImoutoRebirth.Common.Host;

public static class HostBuilderExtensions
{
    extension(IHostBuilder builder)
    {
        public IHostBuilder UseStartup(Func<IConfiguration, BaseStartup> startupFabric)
        {
            ArgumentValidator.NotNull(() => startupFabric);

            builder.ConfigureServices((context, collection) =>
            {
                var startup = startupFabric.Invoke(context.Configuration);
                startup.ConfigureServices(collection);
            });

            return builder;
        }

        public IHostBuilder UseEnvironmentFromEnvironmentVariable(string servicePrefix)
        {
            var environment = Environment.GetEnvironmentVariable($"{servicePrefix}ENVIRONMENT");

            builder.UseEnvironment(environment ?? "Production");

            return builder;
        }

        public IHostBuilder UseConfiguration(string servicePrefix)
        {
            builder.ConfigureAppConfiguration(
                (context, builder1) => builder1.AddJsonFile("appSettings.json", false, true)
                    .AddJsonFile(
                        $"appSettings.{context.HostingEnvironment.EnvironmentName}.json",
                        true,
                        true)
                    .AddEnvironmentVariables(servicePrefix));

            return builder;
        }

        public IHostBuilder SetWorkingDirectory()
        {
            Directory.SetCurrentDirectory(
                Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!);

            return builder;
        }
    }
}
