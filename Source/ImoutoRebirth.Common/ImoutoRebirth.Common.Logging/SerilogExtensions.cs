using System.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using Serilog.Exceptions;

namespace ImoutoRebirth.Common.Logging;

public static class SerilogExtensions
{
    private const string DefaultOpenSearchUri = "http://localhost:9200";

    private const string FileTemplate
        = "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level}] <s:{SourceContext}> {Message}{NewLine}{Exception}";
    private const string ConsoleTemplate
        = "[{Timestamp:HH:mm:ss} {Level:u3}] <s:{SourceContext}> {Message:lj}{NewLine}{Exception}";


    public static LoggerConfiguration WithoutDefaultLoggers(this LoggerConfiguration configuration)
        => configuration.MinimumLevel.Verbose()
            .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
            .MinimumLevel.Override("System", LogEventLevel.Warning);

    public static LoggerConfiguration WithConsole(this LoggerConfiguration configuration)
        => configuration.WriteTo.Console(
            outputTemplate: ConsoleTemplate,
            restrictedToMinimumLevel: LogEventLevel.Verbose);

    public static LoggerConfiguration WithAllRollingFile(
        this LoggerConfiguration configuration,
        string pathFormat = "logs/all.log")
        => configuration.WriteTo.File(
            pathFormat,
            outputTemplate: FileTemplate,
            rollingInterval: RollingInterval.Day,
            restrictedToMinimumLevel: LogEventLevel.Verbose);

    public static LoggerConfiguration WithInformationRollingFile(
        this LoggerConfiguration configuration,
        string pathFormat = "logs/information.log")
        => configuration.WriteTo.File(
            pathFormat,
            outputTemplate: FileTemplate,
            rollingInterval: RollingInterval.Day,
            restrictedToMinimumLevel: LogEventLevel.Information);    
    
    public static LoggerConfiguration WithOpenSearch(
        this LoggerConfiguration configuration,
        IConfiguration appConfiguration,
        IHostEnvironment hostEnvironment)
    {
        var url = appConfiguration.GetValue<string>("OpenSearchUri") ?? DefaultOpenSearchUri;

        if (string.IsNullOrWhiteSpace(url))
            return configuration;
        
        var applicationName = hostEnvironment.ApplicationName.ToLower().Replace('.', '-');

        configuration.Enrich.WithProperty("Application", GetApplicationName(hostEnvironment));
        configuration.Enrich.WithExceptionDetails();
        configuration.Enrich.With<CurrentActivityIdEnricher>();

        var indexFormat = hostEnvironment.IsProduction()
            ? $$"""{{applicationName}}-{0:yyyy.MM}"""
            : $$"""dev-{{applicationName}}-{0:yyyy.MM}""";
        
        return configuration.WriteTo.OpenSearch(
            nodeUris: url,
            indexFormat: indexFormat,
            restrictedToMinimumLevel: LogEventLevel.Verbose);
    }

    private static string GetApplicationName(IHostEnvironment hostEnvironment)
    {
        var appName = hostEnvironment.ApplicationName;

        if (appName.EndsWith(".Host"))
            appName = appName[..^5];

        if (appName.EndsWith(".WebApi"))
            appName = appName[..^7];

        if (appName.StartsWith("ImoutoRebirth."))
            appName = appName[14..];

        return appName;
    }   
}

internal class CurrentActivityIdEnricher : ILogEventEnricher
{
    public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
    {
        var currentActivityId = Activity.Current?.Id;
        var currentActivityTraceStateString = Activity.Current?.TraceStateString;

        var traceId = Activity.Current?.IdFormat == ActivityIdFormat.W3C
            ? Activity.Current.Id?.Split('-')[1]
            : null;

        if (traceId != null)
            logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("TraceId", traceId));

        if (currentActivityId != null)
            logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("TraceParent", currentActivityId));

        if (currentActivityTraceStateString != null && !string.IsNullOrWhiteSpace(currentActivityTraceStateString))
            logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("TraceState", currentActivityTraceStateString));
    }
}
