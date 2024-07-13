using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OpenTelemetry.Exporter;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace ImoutoRebirth.Common.OpenTelemetry;

public static class ServiceCollectionExtensions
{
    private const string OtlpEndpoint = "http://localhost:4318";
    private const int ExportIntervalMilliseconds = 1000;

    public static IServiceCollection AddOpenTelemetry(
        this IServiceCollection services,
        IHostEnvironment environment,
        IConfiguration configuration)
        => services
            .AddMetrics(environment)
            .AddTracing(environment, configuration);

    private static IServiceCollection AddMetrics(
        this IServiceCollection services,
        IHostEnvironment environment)
    {
        var environmentName = environment.EnvironmentName.ToLower();
        var applicationName = GetApplicationName(environment);

        services.AddOpenTelemetry().WithMetrics(
            builder =>
            {
                builder
                    .SetResourceBuilder(ResourceBuilder
                        .CreateDefault()
                        .AddService(applicationName)
                        .AddAttributes([new("environment", environmentName), new("application", applicationName.ToLowerInvariant())]))
                    .AddNpgsql()
                    .AddRuntimeInstrumentation()
                    .AddAspNetCoreInstrumentation()
                    .SetExemplarFilter(ExemplarFilterType.TraceBased)
                    .AddOtlpExporter((options, optionsReader) =>
                    {
                        options.Endpoint = new Uri(OtlpEndpoint);
                        options.Protocol = OtlpExportProtocol.Grpc;
                        options.BatchExportProcessorOptions.ScheduledDelayMilliseconds = ExportIntervalMilliseconds;
                        optionsReader.PeriodicExportingMetricReaderOptions.ExportIntervalMilliseconds = ExportIntervalMilliseconds;
                    });
            });

        return services;
    }
    
    private static IServiceCollection AddTracing(
        this IServiceCollection services,
        IHostEnvironment environment,
        IConfiguration configuration)
    {
        var applicationName = GetApplicationName(environment);

        services.AddOpenTelemetry().WithTracing(
            builder =>
            {
                builder
                    .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService(applicationName))
                    .AddSource("MongoDB.Driver.Core.Extensions.DiagnosticSources")
                    .AddSource("Quartz")
                    .AddSource("MassTransit")
                    .AddHttpClientInstrumentation(
                        options =>
                        {
                            options.RecordException = true;
                            options.FilterHttpRequestMessage = x => x.RequestUri?.Port is not 9200;
                        })
                    .AddAspNetCoreInstrumentation(
                        options =>
                        {
                            options.RecordException = true;
                            options.Filter = context
                                => context.Request.Path.Value?.Contains("health") != true
                                && context.Request.Path.Value?.Contains("metrics") != true;
                        })
                    .AddOtlpExporter(options =>
                    {
                        options.Endpoint = new Uri(OtlpEndpoint);
                        options.Protocol = OtlpExportProtocol.Grpc;
                    });
            });

        return services;
    }
    
    public static IHostApplicationBuilder ConfigureOpenTelemetryLogging(this IHostApplicationBuilder hostBuilder)
    {
        hostBuilder.Logging
            .AddOpenTelemetry(o =>
            {
                o.IncludeScopes = true;
                o.SetResourceBuilder(ResourceBuilder
                    .CreateDefault()
                    .AddService(GetApplicationName(hostBuilder.Environment)));
                o.AddOtlpExporter((options, optionsReader) =>
                {
                    options.Endpoint = new Uri(OtlpEndpoint);
                    options.Protocol = OtlpExportProtocol.Grpc;
                    optionsReader.BatchExportProcessorOptions.ScheduledDelayMilliseconds = ExportIntervalMilliseconds;
                });
            });

        return hostBuilder;
    }

    private static string GetApplicationName(IHostEnvironment environment)
    {
        var name = environment.ApplicationName.ToLowerInvariant();
        
        if (name.EndsWith("service"))
            name = name[..^7];
        
        if (name.StartsWith("imoutorebirth."))
            name = name[14..];
        
        if (name.EndsWith(".host"))
            name = name[..^5];

        return name[0..1].ToUpperInvariant() + name[1..];
    }
}

file static class NpgsqlMeterBuilderExtensions
{
    /// <summary>
    /// https://github.com/dotnet/aspire/blob/681f2e754dc849f96abafcd2829783e69155abf1/src/Components/Aspire.Npgsql/NpgsqlCommon.cs#L14
    /// Only works with reassigned boundaries, the npgsql metrics are reported in seconds
    /// and the default boundaries are in milliseconds.
    /// </summary>
    public static MeterProviderBuilder AddNpgsql(this MeterProviderBuilder builder)
    {
        double[] secondsBuckets = [0, 0.005, 0.01, 0.025, 0.05, 0.075, 0.1, 0.25, 0.5, 0.75, 1, 2.5, 5, 7.5, 10];
        
        return builder
            .AddMeter("Npgsql")
            .AddView("db.client.commands.duration",
                new ExplicitBucketHistogramConfiguration
                {
                    Boundaries = secondsBuckets
                })
            .AddView("db.client.connections.create_time",
                new ExplicitBucketHistogramConfiguration
                {
                    Boundaries = secondsBuckets
                });
    }
}
