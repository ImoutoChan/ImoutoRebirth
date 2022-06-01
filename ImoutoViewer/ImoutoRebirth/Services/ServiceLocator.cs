﻿using ImoutoRebirth.Lilin.WebApi.Client;
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
        sc.AddTransient<IImoutoRebirthRoomWebApiClient>(x 
            => x.GetRequiredService<ImoutoRebirthRoomWebApiClient>());
        sc.AddTransient<IImoutoRebirthLilinWebApiClient>(x 
            => x.GetRequiredService<ImoutoRebirthLilinWebApiClient>());

        sc.AddRoomClient();

        sc.AddSingleton<ImoutoRebirthLilinWebApiClient>(x 
            => new ImoutoRebirthLilinWebApiClient(new Uri("http://miyu:11302/")));

        sc.AddAutoMapper(typeof(ServiceLocator));

        ServiceProvider = sc.BuildServiceProvider();
    }

    private static IServiceCollection AddRoomClient(this IServiceCollection sc)
    {
        sc.AddSingleton<ImoutoRebirthRoomWebApiClient>(x 
            => new ImoutoRebirthRoomWebApiClient(new Uri("http://miyu:11301/")));
        sc.AddTransient<ICollections, global::ImoutoRebirth.Room.WebApi.Client.Collections>();
        sc.AddTransient<IDestinationFolder, global::ImoutoRebirth.Room.WebApi.Client.DestinationFolder>();
        sc.AddTransient<ISourceFolders, global::ImoutoRebirth.Room.WebApi.Client.SourceFolders>();

        return sc;
    }

    public static T GetService<T>() => ServiceProvider.GetService<T>();

    public static T GetRequiredService<T>() 
        => ServiceProvider.GetService<T>() ?? throw new ArgumentException($"Cannot create type {typeof(T).Name}");
}