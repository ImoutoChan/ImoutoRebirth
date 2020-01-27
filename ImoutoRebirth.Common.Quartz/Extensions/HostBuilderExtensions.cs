using System;
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
            Action<IServiceCollection> jobsBuilder = null)
        {
            hostBuilder
               .ConfigureServices(
                    (context, collection)
                        =>
                    {
                        collection
                            .AddQuartz()
                            .AddHostedService<QuartzHostedService>();

                        jobsBuilder(collection);
                    });

            return hostBuilder;
        }
    }
}