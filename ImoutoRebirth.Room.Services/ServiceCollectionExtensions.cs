using ImoutoRebirth.Room.Core.Services;
using ImoutoRebirth.Room.Core.Services.Abstract;
using Microsoft.Extensions.DependencyInjection;

namespace ImoutoRebirth.Room.Core
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddRoomCore(this IServiceCollection services)
        {
            services.AddTransient<IDestinationFolderService, DestinationFolderService>();
            services.AddTransient<ISourceFolderService, SourceFolderService>();
            services.AddTransient<ICollectionFileService, CollectionFileService>();
            services.AddTransient<IFileSystemActualizationService, FileSystemActualizationService>();
            services.AddTransient<IOverseeService, OverseeService>();

            return services;
        }
    }
}