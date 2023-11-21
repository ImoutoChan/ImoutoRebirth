using ImoutoRebirth.Common.Quartz.Extensions;
using ImoutoRebirth.Room.UI.Quartz;
using Microsoft.Extensions.DependencyInjection;

namespace ImoutoRebirth.Room.UI;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddRoomUi(this IServiceCollection services)
    {
        services.AddQuartzJob<OverseeJob, OverseeJob.Description>();
        services.AddMemoryCache();

        return services;
    }
}
