using ImoutoRebirth.Room.UI.Scheduling;
using Microsoft.Extensions.DependencyInjection;

namespace ImoutoRebirth.Room.UI;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddRoomUi(this IServiceCollection services)
    {
        services.Configure<PeriodicRunnerOptions>(x => x.AddJob<OverseeJob>());
        services.AddTransient<OverseeJob>();
        services.AddHostedService<PeriodicRunnerHostedService>();
        
        services.AddMemoryCache();

        return services;
    }
}
