using ImoutoRebirth.Room.Application;
using ImoutoRebirth.Room.Application.Services;
using ImoutoRebirth.Room.DataAccess.Cache;
using ImoutoRebirth.Room.DataAccess.Repositories;
using Microsoft.Extensions.DependencyInjection;

namespace ImoutoRebirth.Room.DataAccess;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddRoomDataAccess(this IServiceCollection services)
    {
        services.AddMemoryCache();
            
        services.AddTransient<ICollectionFileRepository, CollectionFileRepository>();
        services.AddTransient<ICollectionRepository, CollectionRepository>();
        services.AddTransient<ICollectionFileCacheService, CollectionFileCacheService>();

        return services;
    }
}
