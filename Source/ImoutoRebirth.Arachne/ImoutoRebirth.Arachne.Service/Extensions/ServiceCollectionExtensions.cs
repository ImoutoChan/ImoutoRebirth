using ImoutoRebirth.Arachne.MessageContracts;
using ImoutoRebirth.Arachne.MessageContracts.Commands;
using ImoutoRebirth.Arachne.Service.Consumers;
using ImoutoRebirth.Arachne.Service.SearchEngineHistory;
using ImoutoRebirth.Common.MassTransit;
using ImoutoRebirth.Lilin.MessageContracts;
using ImoutoRebirth.Meido.MessageContracts;
using Microsoft.Extensions.DependencyInjection;

namespace ImoutoRebirth.Arachne.Service.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddArachneServices(this IServiceCollection services)
    {
        services.AddTransient<ISearchMetadataCommandHandler, SearchMetadataCommandHandler>();
        services.AddTransient<IMeidoReporter, MeidoReporter>();
        services.AddTransient<EverywhereSearchMetadataCommandConsumer>();
        services.AddTransient<YandereSearchMetadataCommandConsumer>();
        services.AddTransient<DanbooruSearchMetadataCommandConsumer>();
        services.AddTransient<SankakuSearchMetadataCommandConsumer>();
        services.AddTransient<GelbooruSearchMetadataCommandConsumer>();
        services.AddTransient<LoadTagHistoryCommandConsumer>();
        services.AddTransient<LoadNoteHistoryCommandConsumer>();

        services.AddSingleton<TagsSearchEngineHistoryAccessor>();
        services.AddSingleton<NotesSearchEngineHistoryAccessor>();

        return services;
    }

    public static ITrueMassTransitConfigurator AddArachneServicesForRabbit(
        this ITrueMassTransitConfigurator builder)
        => builder
            .AddConsumer<EverywhereSearchMetadataCommandConsumer, IEverywhereSearchMetadataCommand>(ArachneReceiverApp.Name)
            .AddConsumer<YandereSearchMetadataCommandConsumer, IYandereSearchMetadataCommand>(ArachneReceiverApp.Name)
            .AddConsumer<DanbooruSearchMetadataCommandConsumer, IDanbooruSearchMetadataCommand>(ArachneReceiverApp.Name)
            .AddConsumer<SankakuSearchMetadataCommandConsumer, ISankakuSearchMetadataCommand>(ArachneReceiverApp.Name)
            .AddConsumer<GelbooruSearchMetadataCommandConsumer, IGelbooruSearchMetadataCommand>(ArachneReceiverApp.Name)
            .AddConsumer<LoadTagHistoryCommandConsumer, ILoadTagHistoryCommand>(
                ArachneReceiverApp.Name,
                configurator =>
                {
                    configurator.PrefetchCount = 16;
                    configurator.AutoDelete = true;
                    configurator.Durable = false;
                })
            .AddConsumer<LoadNoteHistoryCommandConsumer, ILoadNoteHistoryCommand>(
                ArachneReceiverApp.Name,
                configurator =>
                {
                    configurator.PrefetchCount = 16;
                    configurator.AutoDelete = true;
                    configurator.Durable = false;
                })
            .AddFireAndForget<IUpdateMetadataCommand>(Lilin.MessageContracts.ReceiverApp.Name)
            .AddFireAndForget<ISearchCompleteCommand>(Meido.MessageContracts.MeidoReceiverApp.Name)
            .AddFireAndForget<INotesUpdatedCommand>(Meido.MessageContracts.MeidoReceiverApp.Name)
            .AddFireAndForget<ITagsUpdatedCommand>(Meido.MessageContracts.MeidoReceiverApp.Name);
}
