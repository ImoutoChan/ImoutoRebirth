using System;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Core;

namespace ImoutoRebirth.Common.Logging
{
    public static class HostBuilderExtensions
    {
        public static IWebHostBuilder ConfigureSerilog(
            this IWebHostBuilder webHostBuilder,
            Action<LoggerConfiguration, IConfiguration> configureLogger = null)
        {
            webHostBuilder.ConfigureLogging((context, builder) =>
            {
                builder.ClearProviders();
                builder.AddSerilog(
                    dispose: true, 
                    logger: GetSerilogLogger(context.Configuration, configureLogger));
            });

            return webHostBuilder;
        }

        public static IHostBuilder ConfigureSerilog(
            this IHostBuilder hostBuilder,
            Action<LoggerConfiguration, IConfiguration> configureLogger = null)
        {
            hostBuilder.ConfigureLogging((context, builder) =>
            {
                builder.ClearProviders();
                builder.AddSerilog(
                    dispose: true, 
                    logger: GetSerilogLogger(context.Configuration, configureLogger));
            });

            return hostBuilder;
        }

        private static Logger GetSerilogLogger(
            IConfiguration configuration,
            Action<LoggerConfiguration, IConfiguration> configureLogger)
        {
            var loggerBuilder = new LoggerConfiguration()
                               .Enrich.FromLogContext();

            configureLogger?.Invoke(loggerBuilder, configuration);

            return loggerBuilder.CreateLogger();
        }
    }
}