using ImoutoRebirth.Arachne.MessageContracts;
using ImoutoRebirth.Arachne.MessageContracts.Commands;
using ImoutoRebirth.Common.MassTransit;
using ImoutoRebirth.Common.Quartz.Extensions;
using ImoutoRebirth.Meido.MessageContracts;
using ImoutoRebirth.Meido.UI.Consumers;
using ImoutoRebirth.Meido.UI.Jobs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ImoutoRebirth.Meido.UI;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddMeidoUi(this IServiceCollection services)
    {
        services.AddTransient<NewFileCommandConsumer>();
        services.AddTransient<SearchCompleteCommandConsumer>();
        services.AddTransient<SavedCommandConsumer>();
        services.AddTransient<TagsUpdatedCommandConsumer>();
        services.AddTransient<NotesUpdatedCommandConsumer>();

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

    public static ITrueMassTransitConfigurator AddMeidoServicesForRabbit(
        this ITrueMassTransitConfigurator builder)
    {
        builder
            .AddConsumer<NewFileCommandConsumer, INewFileCommand>(MeidoReceiverApp.Name)
            .AddConsumer<SearchCompleteCommandConsumer, ISearchCompleteCommand>(MeidoReceiverApp.Name)
            .AddConsumer<SavedCommandConsumer, ISavedCommand>(MeidoReceiverApp.Name)
            .AddConsumer<TagsUpdatedCommandConsumer, ITagsUpdatedCommand>(MeidoReceiverApp.Name)
            .AddConsumer<NotesUpdatedCommandConsumer, INotesUpdatedCommand>(MeidoReceiverApp.Name)
            .AddFireAndForget<IYandereSearchMetadataCommand>(ArachneReceiverApp.Name)
            .AddFireAndForget<IDanbooruSearchMetadataCommand>(ArachneReceiverApp.Name)
            .AddFireAndForget<IGelbooruSearchMetadataCommand>(ArachneReceiverApp.Name)
            .AddFireAndForget<IRule34SearchMetadataCommand>(ArachneReceiverApp.Name)
            .AddFireAndForget<ISankakuSearchMetadataCommand>(ArachneReceiverApp.Name)
            .AddFireAndForget<ILoadTagHistoryCommand>(
                ArachneReceiverApp.Name,
                configurator =>
                {
                    configurator.AutoDelete = true;
                    configurator.Durable = false;
                })
            .AddFireAndForget<ILoadNoteHistoryCommand>(
                ArachneReceiverApp.Name,
                configurator =>
                {
                    configurator.AutoDelete = true;
                    configurator.Durable = false;
                });

        return builder;
    }
}
