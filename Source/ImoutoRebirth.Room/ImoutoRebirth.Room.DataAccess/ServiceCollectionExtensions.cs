using ImoutoRebirth.Room.Application.Services;
using ImoutoRebirth.Room.DataAccess.Cache;
using ImoutoRebirth.Room.DataAccess.Repositories;
using ImoutoRebirth.Room.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace ImoutoRebirth.Room.DataAccess;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddRoomDataAccess(this IServiceCollection services)
    {
        services.AddMemoryCache();
        services.AddSingleton<IMd5PresenceCache, Md5PresenceCache>();
        services.AddSingleton<Md5PresenceCacheInvalidator>();
        services.ConfigureDbContext<RoomDbContext>(
            (provider, options) => options.AddInterceptors(provider.GetRequiredService<Md5PresenceCacheInvalidator>()));

        services.AddTransient<ICollectionFileRepository, CollectionFileRepository>();
        services.AddTransient<ICollectionRepository, CollectionRepository>();
        services.AddTransient<ICollectionFileCacheService, CollectionFileCacheService>();
        services.AddTransient<IIntegrityReportRepository, IntegrityReportRepository>();

        return services;
    }
}
