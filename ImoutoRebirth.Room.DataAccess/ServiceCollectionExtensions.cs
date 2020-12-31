using ImoutoRebirth.Room.DataAccess.Cache;
using ImoutoRebirth.Room.DataAccess.Repositories;
using ImoutoRebirth.Room.DataAccess.Repositories.Abstract;
using Microsoft.Extensions.DependencyInjection;

namespace ImoutoRebirth.Room.DataAccess
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddRoomDataAccess(this IServiceCollection services)
        {
            services.AddMemoryCache();
            
            services.AddTransient<ICollectionFileRepository, CollectionFileRepository>();
            services.AddTransient<IDbStateService, DbStateService>();
            services.AddTransient<ICollectionRepository, CollectionRepository>();
            services.AddTransient<ISourceFolderRepository, SourceFolderRepository>();
            services.AddTransient<IDestinationFolderRepository, DestinationFolderRepository>();
            services.AddTransient<ICollectionFileCacheService, CollectionFileCacheService>();

            return services;
        }
    }
}