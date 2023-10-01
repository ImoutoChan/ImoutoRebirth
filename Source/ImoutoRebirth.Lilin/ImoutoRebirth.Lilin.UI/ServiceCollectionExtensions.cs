using ImoutoRebirth.Common.MassTransit;
using ImoutoRebirth.Common.Quartz.Extensions;
using ImoutoRebirth.Lilin.MessageContracts;
using ImoutoRebirth.Lilin.UI.Consumers;
using ImoutoRebirth.Lilin.UI.Quartz;
using ImoutoRebirth.Meido.MessageContracts;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ImoutoRebirth.Lilin.UI;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddLilinUi(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddTransient<UpdateMetadataCommandConsumer>();

        services.AddQuartzJob<RecalculateTagsCountersJob, RecalculateTagsCountersJob.Description>();
        services.AddMemoryCache();

        services.Configure<RecalculateTagCountersSettings>(
            configuration.GetSection(nameof(RecalculateTagCountersSettings)));

        return services;
    }

    public static ITrueMassTransitConfigurator AddMassTransitUi(
        this ITrueMassTransitConfigurator builder)
    {
        builder
            .AddConsumer<UpdateMetadataCommandConsumer, IUpdateMetadataCommand>(Lilin.MessageContracts.ReceiverApp.Name)
            .AddFireAndForget<ISavedCommand>(Meido.MessageContracts.MeidoReceiverApp.Name);

        return builder;
    }
}
