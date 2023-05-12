using ImoutoRebirth.Common.MassTransit;
using ImoutoRebirth.Lilin.MessageContracts;
using ImoutoRebirth.Meido.MessageContracts;
using ImoutoRebirth.Room.Core.Services.Abstract;
using ImoutoRebirth.Room.Infrastructure.Service;
using Microsoft.Extensions.DependencyInjection;
using MeidoReceiverApp = ImoutoRebirth.Meido.MessageContracts.ReceiverApp;
using LilinReceiverApp = ImoutoRebirth.Lilin.MessageContracts.ReceiverApp;

namespace ImoutoRebirth.Room.Infrastructure;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddRoomServices(this IServiceCollection services)
    {
        services.AddTransient<IFileService, FileService>();
        services.AddTransient<IImageService, ImageService>();
        services.AddTransient<IRemoteCommandService, RemoteCommandService>();
        services.AddHttpClient<IImoutoPicsUploader, ImoutoPicsUploader>();

        return services;
    }

    public static ITrueMassTransitConfigurator AddRoomServicesForRabbit(this ITrueMassTransitConfigurator builder)
        => builder
            .AddFireAndForget<INewFileCommand>(MeidoReceiverApp.Name)
            .AddFireAndForget<IUpdateMetadataCommand>(LilinReceiverApp.Name);
}
