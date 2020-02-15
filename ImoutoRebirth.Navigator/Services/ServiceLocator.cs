using System;
using AutoMapper;
using ImoutoRebirth.Lilin.WebApi.Client;
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

            sc.AddSingleton<ICollectionService, CollectionService>();
            sc.AddSingleton<ICollections, Collections>();
            sc.AddSingleton<IFileService, FileService>();
            sc.AddTransient<IImoutoRebirthRoomWebApiClient>(x => x.GetRequiredService<ImoutoRebirthRoomWebApiClient>());
            sc.AddTransient<IImoutoRebirthLilinWebApiClient>(x => x.GetRequiredService<ImoutoRebirthLilinWebApiClient>());

            sc.AddSingleton<ImoutoRebirthRoomWebApiClient>(x 
                => new ImoutoRebirthRoomWebApiClient(new Uri("http://miyu:11301/")));

            sc.AddSingleton<ImoutoRebirthLilinWebApiClient>(x 
                => new ImoutoRebirthLilinWebApiClient(new Uri("http://miyu:11302/")));

            sc.AddAutoMapper(typeof(ServiceLocator));

            ServiceProvider = sc.BuildServiceProvider();
        }

        public static T GetService<T>() => ServiceProvider.GetService<T>();
    }
}