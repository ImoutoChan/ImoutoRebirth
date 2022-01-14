using System;
using ImoutoRebirth.Lilin.WebApi.Client;
using ImoutoRebirth.Navigator.Services.Collections;
using ImoutoRebirth.Navigator.Services.ImoutoViewer;
using ImoutoRebirth.Navigator.Services.Tags;
using ImoutoRebirth.Room.WebApi.Client;
using Microsoft.Extensions.DependencyInjection;

namespace ImoutoRebirth.Navigator.Services
{
    public static class ServiceLocator
    {
        private static readonly IServiceProvider ServiceProvider;

        static ServiceLocator()
        {
            var sc = new ServiceCollection();

            sc.AddTransient<IImoutoViewerService, ImoutoViewerService>();

            sc.AddTransient<ICollectionService, CollectionService>();
            sc.AddTransient<IDestinationFolderService, DestinationFolderService>();
            sc.AddTransient<ISourceFolderService, SourceFolderService>();


            sc.AddTransient<IFileService, FileService>();
            sc.AddTransient<IFileTagService, FileTagService>();
            sc.AddTransient<ITagService, TagService>();
            sc.AddTransient<IFileLoadingService, FileLoadingService>();
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
            sc.AddTransient<ICollections, Room.WebApi.Client.Collections>();
            sc.AddTransient<IDestinationFolder, Room.WebApi.Client.DestinationFolder>();
            sc.AddTransient<ISourceFolders, Room.WebApi.Client.SourceFolders>();

            return sc;
        }

        public static T GetService<T>() => ServiceProvider.GetService<T>();
    }
}