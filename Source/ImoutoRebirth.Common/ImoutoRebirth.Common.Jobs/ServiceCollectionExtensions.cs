using Microsoft.Extensions.DependencyInjection;

namespace ImoutoRebirth.Common.Jobs;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddRunningJobs<TJobType>(this IServiceCollection services)
    {
        services.AddSingleton(typeof(IRunningJobs<>), typeof(RunningJobs<>));

        return services;
    }
}
