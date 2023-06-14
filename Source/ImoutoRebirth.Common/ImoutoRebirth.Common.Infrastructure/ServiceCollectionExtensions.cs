using ImoutoRebirth.Common.Application;
using Microsoft.Extensions.DependencyInjection;

namespace ImoutoRebirth.Common.Infrastructure;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddDistributedBus(this IServiceCollection services)
    {
        services.AddTransient<IDistributedEventsPublisher, DistributedEventsPublisher>();
        services.AddTransient<IDistributedCommandBus, DistributedCommandBus>();

        return services;
    }
}
