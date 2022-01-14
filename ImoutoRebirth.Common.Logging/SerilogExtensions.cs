using Microsoft.Extensions.Configuration;
using Serilog;
using Serilog.Events;

namespace ImoutoRebirth.Common.Logging
{
    public static class SerilogExtensions
    {
        private const string _fileTemplate
            = "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level}] <s:{SourceContext}> {Message}{NewLine}{Exception}";
        private const string _consoleTemplate
            = "[{Timestamp:HH:mm:ss} {Level:u3}] <s:{SourceContext}> {Message:lj}{NewLine}{Exception}";


        public static LoggerConfiguration WithoutDefaultLoggers(this LoggerConfiguration configuration)
            => configuration.MinimumLevel.Verbose()
                            .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                            .MinimumLevel.Override("System", LogEventLevel.Warning);

        public static LoggerConfiguration WithConsole(this LoggerConfiguration configuration)
            => configuration.WriteTo.Console(
                outputTemplate: _consoleTemplate,
                restrictedToMinimumLevel: LogEventLevel.Verbose);

        public static LoggerConfiguration WithAllRollingFile(
            this LoggerConfiguration configuration,
            string pathFormat = "logs/{Date}-all.log")
            => configuration.WriteTo.File(
                pathFormat,
                outputTemplate: _fileTemplate,
                restrictedToMinimumLevel: LogEventLevel.Verbose);

        public static LoggerConfiguration WithInformationRollingFile(
            this LoggerConfiguration configuration,
            string pathFormat = "logs/{Date}-information.log")
            => configuration.WriteTo.File(
                pathFormat,
                outputTemplate: _fileTemplate,
                restrictedToMinimumLevel: LogEventLevel.Information);

        public static LoggerConfiguration PatchWithConfiguration(
            this LoggerConfiguration configuration,
            IConfiguration appConfiguration)
            => configuration.ReadFrom.Configuration(appConfiguration);
    }
}
