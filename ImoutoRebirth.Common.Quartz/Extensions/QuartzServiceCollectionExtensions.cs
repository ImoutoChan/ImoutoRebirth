using System;
using Microsoft.Extensions.DependencyInjection;
using Quartz;
using Quartz.Impl;
using Quartz.Spi;

namespace ImoutoRebirth.Common.Quartz.Extensions
{
    public static class QuartzServiceCollectionExtensions
    {
        public static IServiceCollection AddQuartz(this IServiceCollection serviceCollection)
        {
            serviceCollection.AddSingleton<IJobFactory, QuartzJobFactory>();
            serviceCollection.AddSingleton<QuartzStartup>();
            serviceCollection.AddSingleton<IScheduler>(CreateSchedule);

            return serviceCollection;
        }

        public static IServiceCollection AddQuartzJob<TJob, TJobDescription>(
            this IServiceCollection serviceCollection,
            ServiceLifetime lifetime = ServiceLifetime.Transient)
            where TJob : IJob
            where TJobDescription : IQuartzJobDescription
        {
            serviceCollection.Add(new ServiceDescriptor(typeof(TJob), typeof(TJob), lifetime));
            serviceCollection.Add(new ServiceDescriptor(typeof(IQuartzJobDescription), typeof(TJobDescription), lifetime));

            return serviceCollection;
        }

        private static IScheduler CreateSchedule(IServiceProvider provider)
        {
            var schedulerFactory = new StdSchedulerFactory();
            var jobFactory = provider.GetRequiredService<IJobFactory>();

            var scheduler = schedulerFactory.GetSchedulerSynchronously();
            scheduler.JobFactory = jobFactory;

            return scheduler;
        }
    }
}