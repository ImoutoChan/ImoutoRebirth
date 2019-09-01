using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace ImoutoRebirth.Arachne.Host
{
    public static class BooruLoaderConfigurationExtensions
    {
        public static IHost ConfigureBooruParserLogging(this IHost host)
        {
            var loggerFactory = host.Services.GetRequiredService<ILoggerFactory>();

            Imouto.BooruParser.LoggerAccessor.LoggerFactory = loggerFactory;

            return host;
        }
    }
}