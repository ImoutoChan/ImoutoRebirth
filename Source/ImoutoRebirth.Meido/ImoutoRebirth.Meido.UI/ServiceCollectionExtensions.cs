using ImoutoRebirth.Arachne.MessageContracts;
using ImoutoRebirth.Common.MassTransit;
using ImoutoRebirth.Common.Quartz.Extensions;
using ImoutoRebirth.Meido.UI.Jobs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ImoutoRebirth.Meido.UI;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddMeidoUi(this IServiceCollection services)
    {
        services.AddQuartzJob<MetaActualizerJob, MetaActualizerJob.Description>();
        services.AddQuartzJob<FaultToleranceJob, FaultToleranceJob.Description>();

        return services;
    }

    public static IServiceCollection ConfigureMeidoUi(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<MetadataActualizerSettings>(
            configuration.GetSection(nameof(MetadataActualizerSettings)));

        services.Configure<FaultToleranceSettings>(
            configuration.GetSection(nameof(FaultToleranceSettings)));

        return services;
    }

    public static MassTransitConfigurator AddMeidoMassTransitSetup(
        this MassTransitConfigurator builder)
        => builder
            .AddCommand<YandereSearchMetadataCommand>()
            .AddCommand<DanbooruSearchMetadataCommand>()
            .AddCommand<GelbooruSearchMetadataCommand>()
            .AddCommand<Rule34SearchMetadataCommand>()
            .AddCommand<SankakuSearchMetadataCommand>()
            .AddCommand<ExHentaiSearchMetadataCommand>()
            .AddCommand<LoadTagHistoryCommand>()
            .AddCommand<LoadNoteHistoryCommand>();
}
