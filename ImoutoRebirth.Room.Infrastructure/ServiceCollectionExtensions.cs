using ImoutoRebirth.Arachne.MessageContracts;
using ImoutoRebirth.Arachne.MessageContracts.Commands;
using ImoutoRebirth.Room.Core.Services.Abstract;
using ImoutoRebirth.Room.Infrastructure.Service;
using MassTransit.RabbitMq.Extensions.Hosting.Contracts;
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

        public static IMassTransitRabbitMqHostingBuilder AddRoomServicesForRabbit(
            this IMassTransitRabbitMqHostingBuilder builder)
            => builder.WithFireAndForgetSendEndpoint<IEverywhereSearchMetadataCommand>(ReceiverApp.Name);
    }
}