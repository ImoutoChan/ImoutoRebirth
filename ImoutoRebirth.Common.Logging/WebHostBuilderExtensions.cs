using System;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Core;

namespace ImoutoRebirth.Common.Logging
{
    public static class WebHostBuilderExtensions
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