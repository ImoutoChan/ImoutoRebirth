using System;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace ImoutoRebirth.Common.Quartz.Extensions
{
    public static class HostBuilderExtensions
    {
        /// <summary>
        /// Use this method to add Quartz hosted service.
        /// You don't need to register any additional dependencies except for jobs and job descriptions.
        /// </summary>
        /// <param name="hostBuilder">Host builder to use.</param>
        /// <param name="jobsBuilder">You can register your jobs here with method AddQuartzJob.</param>
        /// <returns>Initial Host builder.</returns>
        public static IHostBuilder UseQuartz(
            this IHostBuilder hostBuilder, 
            Func<IServiceCollection> jobsBuilder = null)
        {
            hostBuilder
               .ConfigureServices(
                    (context, collection)
                        => collection.AddQuartz()
                                     .AddHostedService<QuartzHostedService>());

            return hostBuilder;
        }

        /// <summary>
        /// Use this method to add Quartz hosted service.
        /// You don't need to register any additional dependencies except for jobs and job descriptions.
        /// </summary>
        /// <param name="webhostBuilder">WebHost builder to use.</param>
        /// <param name="jobsBuilder">You can register your jobs here with method AddQuartzJob.</param>
        /// <returns>Initial WebHost builder.</returns>
        public static IWebHostBuilder UseQuartz(
            this IWebHostBuilder webhostBuilder, 
            Func<IServiceCollection> jobsBuilder = null)
        {
            webhostBuilder
               .ConfigureServices(
                    (context, collection)
                        => collection.AddQuartz()
                                     .AddHostedService<QuartzHostedService>());

            return webhostBuilder;
        }
    }
}