using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Core;

namespace ImoutoRebirth.Common.Logging;

public static class HostBuilderExtensions
{
    public static IHostBuilder ConfigureSerilog(
        this IHostBuilder hostBuilder,
        Action<LoggerConfiguration, IConfiguration, IHostEnvironment>? configureLogger = null)
    {
        hostBuilder.ConfigureLogging((context, builder) =>
        {
            builder.ClearProviders();
            builder.AddSerilog(
                dispose: true, 
                logger: GetSerilogLogger(context.Configuration, context.HostingEnvironment, configureLogger));
        });

        return hostBuilder;
    }

    private static Logger GetSerilogLogger(
        IConfiguration configuration, 
        IHostEnvironment hostEnvironment,
        Action<LoggerConfiguration, IConfiguration, IHostEnvironment>? configureLogger)
    {
        var loggerBuilder = new LoggerConfiguration()
            .Enrich.FromLogContext();

        configureLogger?.Invoke(loggerBuilder, configuration, hostEnvironment);

        return loggerBuilder.CreateLogger();
    }
}
