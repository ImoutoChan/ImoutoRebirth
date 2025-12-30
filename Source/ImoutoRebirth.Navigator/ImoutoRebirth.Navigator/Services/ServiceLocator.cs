using CommunityToolkit.Mvvm.Messaging;
using ImoutoRebirth.Common.AutoMapper;
using ImoutoRebirth.Lilin.WebApi.Client;
using ImoutoRebirth.Navigator.Services.Collections;
using ImoutoRebirth.Navigator.Services.ImoutoViewer;
using ImoutoRebirth.Navigator.Services.Tags;
using ImoutoRebirth.Navigator.Slices.IntegrityReport.Services;
using ImoutoRebirth.Navigator.Slices.Updates.Services;
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
        sc.AddTransient<IIntegrityReportService, IntegrityReportService>();

        sc.AddTransient<IFileService, FileService>();
        sc.AddTransient<IFileTagService, FileTagService>();
        sc.AddTransient<IFileNoteService, FileNoteService>();
        sc.AddTransient<ITagService, TagService>();
        sc.AddTransient<IFileLoadingService, FileLoadingService>();

        sc.AddTransient<ISettingsUpgradeService, SettingsUpgradeService>();

        sc.AddHttpClient<IUpdateService, UpdateService>(client =>
        {
            client.DefaultRequestHeaders.Add("User-Agent", "ImoutoRebirth-Updater");
            client.Timeout = TimeSpan.FromSeconds(30);
        });

        sc.AddRoomWebApiClients(Settings.Default.RoomHost);
        sc.AddLilinWebApiClients(Settings.Default.LilinHost);

        sc.AddSingleton<IMessenger>(new WeakReferenceMessenger());

        AutoMapperIssue.Fix();
        sc.AddAutoMapper(x => x.AddProfile<AutoMapperProfile>());

        ServiceProvider = sc.BuildServiceProvider();
    }

    public static T GetService<T>() where T : notnull 
        => ServiceProvider.GetRequiredService<T>();

    public static IMessenger GetMessenger() 
        => ServiceProvider.GetRequiredService<IMessenger>();
}
