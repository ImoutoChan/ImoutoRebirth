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

        sc.AddLilinWebApiClients("http://localhost:11302/");
        sc.AddRoomWebApiClients("http://localhost:11301/");

        AutoMapperFix.Fix();
        sc.AddAutoMapper(x => x.AddProfile<AutoMapperProfile>());

        ServiceProvider = sc.BuildServiceProvider();
    }

    public static T? GetService<T>() => ServiceProvider.GetService<T>();

    public static T GetRequiredService<T>() where T : notnull 
        => ServiceProvider.GetRequiredService<T>();
}
