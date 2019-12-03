﻿using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using ImoutoRebirth.Common.Host;
using ImoutoRebirth.Common.Logging;
using Microsoft.Extensions.Hosting;

namespace ImoutoRebirth.Arachne.Host
{
    internal class Program
    {
        private const string ServicePrefix = "ARACHNE_";

        private static async Task Main(string[] args)
        {
            Directory.SetCurrentDirectory(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location));

            await CreateConsoleHost(args)
                .ConfigureBooruParserLogging()
                .RunAsync();
        }

        public static IHost CreateConsoleHost(string[] args)
            => new HostBuilder()
              .UseWindowsService()
              .UseEnvironmentFromEnvironmentVariable(ServicePrefix)
              .UseConfiguration(ServicePrefix)
              .ConfigureSerilog(
                   (loggerBuilder, appConfiguration)
                       => loggerBuilder
                         .WithoutDefaultLoggers()
                         .WithConsole()
                         .WithAllRollingFile()
                         .WithInformationRollingFile()
                         .PatchWithConfiguration(appConfiguration))
              .UseStartup(x => new Startup(x))
              .Build();
    }
}