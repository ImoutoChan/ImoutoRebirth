using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Core;

namespace ImoutoRebirth.Common.Logging;

public static class WebApplicationBuilderExtensions
{
    public static WebApplicationBuilder ConfigureSerilog(
        this WebApplicationBuilder hostBuilder,
        Action<LoggerConfiguration, IConfiguration, IHostEnvironment>? configureLogger = null)
    {
        hostBuilder.Logging
            .ClearProviders()
            .AddSerilog(
                dispose: true, 
                logger: GetSerilogLogger(hostBuilder.Configuration, hostBuilder.Environment, configureLogger));

        return hostBuilder;
    }

    public static HostApplicationBuilder ConfigureSerilog(
        this HostApplicationBuilder hostBuilder,
        Action<LoggerConfiguration, IConfiguration, IHostEnvironment>? configureLogger = null)
    {
        hostBuilder.Logging
            .ClearProviders()
            .AddSerilog(
                dispose: true, 
                logger: GetSerilogLogger(hostBuilder.Configuration, hostBuilder.Environment, configureLogger));

        return hostBuilder;
    }

    private static Logger GetSerilogLogger(
        IConfiguration configuration, 
        IHostEnvironment hostEnvironment,
        Action<LoggerConfiguration, IConfiguration, IHostEnvironment>? configureLogger)
    {
        var loggerBuilder =
            new LoggerConfiguration()
                .Enrich.FromLogContext();

        configureLogger?.Invoke(loggerBuilder, configuration, hostEnvironment);

        return loggerBuilder.CreateLogger();
    }
}
