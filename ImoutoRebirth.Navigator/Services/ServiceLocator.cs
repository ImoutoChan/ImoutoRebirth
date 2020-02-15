using System;
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

            sc.AddSingleton<ImoutoRebirthRoomWebApiClient>(x 
                => new ImoutoRebirthRoomWebApiClient(new Uri("https://localhost:11301/")));

            ServiceProvider = sc.BuildServiceProvider();
        }

        public static T GetService<T>() => ServiceProvider.GetService<T>();
    }
}