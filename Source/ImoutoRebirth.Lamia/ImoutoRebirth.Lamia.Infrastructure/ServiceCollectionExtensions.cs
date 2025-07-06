using ImoutoRebirth.Common.Infrastructure;
using ImoutoRebirth.Common.MassTransit;
using ImoutoRebirth.Lamia.Application.Services;
using ImoutoRebirth.Lamia.Infrastructure.Meta;
using ImoutoRebirth.Lamia.Infrastructure.Meta.Providers;
using ImoutoRebirth.Lilin.MessageContracts;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ImoutoRebirth.Lamia.Infrastructure;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddLamiaInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddTransient<IMetadataProvider, MetadataProvider>();
        services.AddTransient<IMetadataForFileProvider, VideoMetadataProvider>();
        services.AddTransient<IMetadataForFileProvider, ImageMetadataProvider>();
        services.AddTransient<IMetadataForFileProvider, ArchiveMetadataProvider>();

        services.AddDistributedBus();

        services.Configure<FFmpegOptions>(configuration.GetSection("FFmpeg"));
        services.AddTransient<IFFmpegAccessor, FFmpegAccessor>();

        return services;
    }

    public static MassTransitConfigurator AddLamiaMassTransitSetup(this MassTransitConfigurator builder)
        => builder
            .AddCommand<UpdateMetadataCommand>();
}
