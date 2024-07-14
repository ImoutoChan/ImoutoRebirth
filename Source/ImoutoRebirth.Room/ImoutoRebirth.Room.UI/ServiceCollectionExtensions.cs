using ImoutoRebirth.Room.Application.Services;
using ImoutoRebirth.Room.UI.FileSystemEvents;
using Microsoft.Extensions.DependencyInjection;

namespace ImoutoRebirth.Room.UI;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddRoomUi(this IServiceCollection services)
    {
        services.AddHostedService<SourceFolderWatcherHostedService>();

        services.AddSingleton<SourceFolderWatcher>();
        services.AddTransient<ISourceFolderWatcher>(x => x.GetRequiredService<SourceFolderWatcher>());
        
        services.AddMemoryCache();

        return services;
    }
}
