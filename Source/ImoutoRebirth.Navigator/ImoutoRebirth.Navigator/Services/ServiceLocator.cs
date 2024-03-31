using ImoutoRebirth.Lilin.WebApi.Client;
using ImoutoRebirth.Navigator.Services.Collections;
using ImoutoRebirth.Navigator.Services.ImoutoViewer;
using ImoutoRebirth.Navigator.Services.Tags;
using ImoutoRebirth.Room.WebApi.Client;
using Microsoft.Extensions.DependencyInjection;

namespace ImoutoRebirth.Navigator.Services;

public static class ServiceLocator
{
    private static readonly IServiceProvider ServiceProvider;

    static ServiceLocator()
    {
        var sc = new ServiceCollection();

        sc.AddTransient<IRoomCache, RoomCache>();
        sc.AddMemoryCache();

        sc.AddTransient<IImoutoViewerService, ImoutoViewerService>();

        sc.AddTransient<ICollectionService, CollectionService>();
        sc.AddTransient<IDestinationFolderService, DestinationFolderService>();
        sc.AddTransient<ISourceFolderService, SourceFolderService>();
        sc.AddTransient<IImoutoPicsUploaderStateService, ImoutoPicsUploaderStateService>();

        sc.AddTransient<IFileService, FileService>();
        sc.AddTransient<IFileTagService, FileTagService>();
        sc.AddTransient<IFileNoteService, FileNoteService>();
        sc.AddTransient<ITagService, TagService>();
        sc.AddTransient<IFileLoadingService, FileLoadingService>();

        sc.AddRoomWebApiClients(Settings.Default.RoomHost);
        sc.AddLilinWebApiClients(Settings.Default.LilinHost);

        sc.AddAutoMapper(typeof(ServiceLocator));

        ServiceProvider = sc.BuildServiceProvider();
    }

    public static T GetService<T>() where T : notnull 
        => ServiceProvider.GetRequiredService<T>();
}
