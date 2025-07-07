using ImoutoRebirth.Common.Domain;
using ImoutoRebirth.Common.MassTransit;
using ImoutoRebirth.Lamia.MessageContracts;
using ImoutoRebirth.Lilin.MessageContracts;
using ImoutoRebirth.Meido.MessageContracts;
using ImoutoRebirth.Room.Application.Services;
using ImoutoRebirth.Room.Infrastructure.Service;
using Microsoft.Extensions.DependencyInjection;

namespace ImoutoRebirth.Room.Infrastructure;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddRoomInfrastructure(this IServiceCollection services)
    {
        services.AddTransient<IImageService, ImageService>();
        services.AddTransient<IRemoteCommandService, RemoteCommandService>();
        services.AddHttpClient<IImoutoPicsUploader, ImoutoPicsUploader>();
        services.AddScoped<IEventStorage, EventStorage>();

        return services;
    }
    
    public static MassTransitConfigurator AddRoomMassTransitSetup(
        this MassTransitConfigurator builder)
        => builder
            .AddCommand<NewFileCommand>()
            .AddCommand<ExtractFileMetadataCommand>()
            .AddCommand<UpdateMetadataCommand>();
}
