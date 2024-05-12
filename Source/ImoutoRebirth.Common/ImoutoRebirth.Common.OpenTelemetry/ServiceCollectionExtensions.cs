using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Npgsql;
using OpenTelemetry.Exporter;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace ImoutoRebirth.Common.OpenTelemetry;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddOpenTelemetry(
        this IServiceCollection services,
        IHostEnvironment environment,
        IConfiguration configuration)
        => services
            .AddMetrics(environment)
            .AddTracing(environment, configuration);

    public static IApplicationBuilder UseOpenTelemetry(this IApplicationBuilder app)
        => app.UseOpenTelemetryPrometheusScrapingEndpoint();

    public static IServiceCollection AddMetrics(
        this IServiceCollection services,
        IHostEnvironment environment)
    {
        var environmentName = environment.EnvironmentName.ToLower();
        var applicationName = GetApplicationName(environment);

        services.AddOpenTelemetry().WithMetrics(
            builder =>
            {
                builder
                    .AddMeter(environment.ApplicationName)
                    .SetResourceBuilder(ResourceBuilder
                        .CreateDefault()
                        .AddService(applicationName)
                        .AddAttributes([new("environment", environmentName), new("application", applicationName)]))
                    .AddRuntimeInstrumentation()
                    .AddAspNetCoreInstrumentation()
                    .AddPrometheusExporter()
                    .AddOtlpExporter();
            });

        return services;
    }

    public static IServiceCollection AddTracing(
        this IServiceCollection services,
        IHostEnvironment environment,
        IConfiguration configuration)
    {
        services.Configure<JaegerExporterOptions>(configuration.GetSection("Jaeger"));
        var applicationName = GetApplicationName(environment);

        services.AddOpenTelemetry().WithTracing(
            builder =>
            {
                builder
                    .SetResourceBuilder(
                        ResourceBuilder.CreateDefault()
                            .AddService(applicationName))
                    .AddSource("MongoDB.Driver.Core.Extensions.DiagnosticSources")
                    .AddSource("Quartz")
                    .AddSource("MassTransit")
                    .AddNpgsql()
                    .AddHttpClientInstrumentation(
                        options =>
                        {
                            options.RecordException = true;
                            options.FilterHttpRequestMessage = x
                                => x.RequestUri?.Port is not 9200;
                        })
                    .AddAspNetCoreInstrumentation(
                        options =>
                        {
                            options.RecordException = true;
                            options.Filter = context
                                => context.Request.Path.Value?.Contains("health") != true
                                && context.Request.Path.Value?.Contains("metrics") != true;
                        })
                    .AddJaegerExporter();
            });

        return services;
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

        return name;
    }
}
