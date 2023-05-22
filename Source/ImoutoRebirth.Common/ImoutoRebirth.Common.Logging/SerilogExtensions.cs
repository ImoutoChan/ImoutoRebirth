using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;

namespace ImoutoRebirth.Common.Logging;

public static class SerilogExtensions
{
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
        var url = appConfiguration.GetValue<string>("OpenSearchUri") ?? "http://localhost:9200/";
        var applicationName = hostEnvironment.ApplicationName.ToLower().Replace('.', '-');

        configuration.Enrich.WithProperty("Application", hostEnvironment.ApplicationName);

        var indexFormat = hostEnvironment.IsProduction()
            ? $$"""{{applicationName}}-{0:yyyy.MM}"""
            : $$"""dev-{{applicationName}}-{0:yyyy.MM}""";
        
        return configuration.WriteTo.OpenSearch(
            nodeUris: url,
            indexFormat: indexFormat,
            restrictedToMinimumLevel: LogEventLevel.Verbose);
    }
}
