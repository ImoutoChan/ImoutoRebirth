using ImoutoRebirth.Common;
using ImoutoRebirth.Room.Application.Services;
using ImoutoRebirth.Room.UI.FileSystemEvents;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ImoutoRebirth.Room.UI;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddRoomUi(this IServiceCollection services, IConfiguration configuration)
    {
        var isHostedServiceDisabled = configuration.GetValue<bool?>("SourceFolderWatcherHostedServiceDisabled") ?? false;

        if (!isHostedServiceDisabled)
            services.AddHostedService<SourceFolderWatcherHostedService>();

        services.AddHostedService<IntegrityReportJobBackgroundService>();

        services.AddSingleton<SourceFolderWatcher>();
        services.AddTransient<ISourceFolderWatcher>(x => x.GetRequiredService<SourceFolderWatcher>());
        
        services.AddMemoryCache();

        return services;
    }
}
