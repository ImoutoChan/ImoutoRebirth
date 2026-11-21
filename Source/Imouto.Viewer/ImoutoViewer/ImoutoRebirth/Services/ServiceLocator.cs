using System.IO;
using System.Text.Json;
using ImoutoRebirth.Common.AutoMapper;
using ImoutoRebirth.Lilin.WebApi.Client;
using ImoutoRebirth.Room.WebApi.Client;
using ImoutoViewer.ImoutoRebirth.Services.Collections;
using ImoutoViewer.ImoutoRebirth.Services.Tags;
using Microsoft.Extensions.DependencyInjection;

namespace ImoutoViewer.ImoutoRebirth.Services;

public static class ServiceLocator
{
    private static readonly IServiceProvider ServiceProvider;

    static ServiceLocator()
    {
        var sc = new ServiceCollection();
            
        sc.AddTransient<ICollectionService, CollectionService>();
        sc.AddTransient<IDestinationFolderService, DestinationFolderService>();
        sc.AddTransient<ISourceFolderService, SourceFolderService>();


        sc.AddTransient<IFileService, FileService>();
        sc.AddTransient<IFileLoadingService, FileLoadingService>();
        sc.AddTransient<IFileTagService, FileTagService>();
        sc.AddTransient<ITagService, TagService>();
        //sc.AddTransient<IFileLoadingService, FileLoadingService>();

        var settingsPath = Path.Combine(AppContext.BaseDirectory, "appsettings.Production.json");
        var endpoints = File.Exists(settingsPath)
            ? JsonSerializer.Deserialize<ViewerEndpoints>(File.ReadAllText(settingsPath))
            : null;

        sc.AddLilinWebApiClients(endpoints?.LilinUrl ?? "http://localhost:11302/");
        sc.AddRoomWebApiClients(endpoints?.RoomUrl ?? "http://localhost:11301/");

        AutoMapperIssue.Fix();
        sc.AddAutoMapper(x => x.AddProfile<AutoMapperProfile>());

        ServiceProvider = sc.BuildServiceProvider();
    }

    private record ViewerEndpoints(string? RoomUrl, string? LilinUrl);

    public static T? GetService<T>() => ServiceProvider.GetService<T>();

    public static T GetRequiredService<T>() where T : notnull 
        => ServiceProvider.GetRequiredService<T>();
}
