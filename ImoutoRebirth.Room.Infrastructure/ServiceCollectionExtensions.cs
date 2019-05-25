using ImoutoRebirth.Common.MassTransit;
using ImoutoRebirth.Meido.MessageContracts;
using ImoutoRebirth.Room.Core.Services.Abstract;
using ImoutoRebirth.Room.Infrastructure.Service;
using Microsoft.Extensions.DependencyInjection;

namespace ImoutoRebirth.Room.Infrastructure
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddRoomServices(this IServiceCollection services)
        {
            services.AddTransient<IFileService, FileService>();
            services.AddTransient<IImageService, ImageService>();
            services.AddTransient<IRemoteCommandService, RemoteCommandService>();

            return services;
        }

        public static ITrueMassTransitConfigurator AddRoomServicesForRabbit(this ITrueMassTransitConfigurator builder) 
            => builder.AddFireAndForget<INewFileCommand>(ReceiverApp.Name);
    }
}