﻿using System.Threading.Tasks;
using ImoutoRebirth.Common.Host;
using Microsoft.Extensions.Hosting;

namespace ImoutoRebirth.Arachne.Host
{
    internal class Program
    {
        private const string ServicePrefix = "ARACHNE_";

        private static async Task Main(string[] args)
        {
            await CreateConsoleHost(args).RunAsync();
        }

        public static IHost CreateConsoleHost(string[] args)
            => new HostBuilder()
              .UseConsoleLifetime()
              .UseEnvironmentFromEnvironmentVariable(ServicePrefix)
              .UseConfiguration(ServicePrefix)
              .UseStartup(x => new Startup(x))
              .Build();
    }
}
