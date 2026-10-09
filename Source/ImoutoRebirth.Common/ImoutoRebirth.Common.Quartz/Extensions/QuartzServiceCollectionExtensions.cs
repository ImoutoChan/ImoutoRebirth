using Microsoft.Extensions.DependencyInjection;
using Quartz;

namespace ImoutoRebirth.Common.Quartz.Extensions;

public static class QuartzServiceCollectionExtensions
{
    internal static IServiceCollection AddQuartz(this IServiceCollection services)
    {
        global::Quartz.QuartzServiceCollectionExtensions.AddQuartz(services);
        services.AddQuartzHostedService(x => x.WaitForJobsToComplete = true);
        services.AddHostedService<QuartzJobsScheduler>();

        return services;
    }

    public static IServiceCollection AddQuartzJob<TJob, TJobDescription>(
        this IServiceCollection services,
        Func<IServiceProvider, TJobDescription>? factory = null,
        ServiceLifetime lifetime = ServiceLifetime.Transient)
        where TJob : IJob
        where TJobDescription : IQuartzJobDescription
    {
        services.Add(new ServiceDescriptor(typeof(TJob), typeof(TJob), lifetime));

        services.Add(
            factory == null
                ? new ServiceDescriptor(typeof(IQuartzJobDescription), typeof(TJobDescription), lifetime)
                : new ServiceDescriptor(typeof(IQuartzJobDescription), x => factory(x), lifetime));

        return services;
    }
}
