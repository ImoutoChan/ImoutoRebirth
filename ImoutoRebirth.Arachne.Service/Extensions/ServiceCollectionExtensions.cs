using ImoutoRebirth.Arachne.MessageContracts.Commands;
using ImoutoRebirth.Arachne.Service.Consumers;
using ImoutoRebirth.Arachne.Service.SearchEngineHistory;
using ImoutoRebirth.Common.MassTransit;
using ImoutoRebirth.Lilin.MessageContracts;
using ImoutoRebirth.Meido.MessageContracts;
using Microsoft.Extensions.DependencyInjection;
using ReceiverApp = ImoutoRebirth.Arachne.MessageContracts.ReceiverApp;

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
        services.AddTransient<LoadTagHistoryCommandConsumer>();
        services.AddTransient<LoadNoteHistoryCommandConsumer>();

        services.AddSingleton<TagsSearchEngineHistoryAccessor>();
        services.AddSingleton<NotesSearchEngineHistoryAccessor>();

        return services;
    }

    public static ITrueMassTransitConfigurator AddArachneServicesForRabbit(
        this ITrueMassTransitConfigurator builder)
        => builder
            .AddConsumer<EverywhereSearchMetadataCommandConsumer, IEverywhereSearchMetadataCommand>(
                ReceiverApp.Name)
            .AddConsumer<YandereSearchMetadataCommandConsumer, IYandereSearchMetadataCommand>(ReceiverApp.Name)
            .AddConsumer<DanbooruSearchMetadataCommandConsumer, IDanbooruSearchMetadataCommand>(ReceiverApp.Name)
            .AddConsumer<SankakuSearchMetadataCommandConsumer, ISankakuSearchMetadataCommand>(ReceiverApp.Name)
            .AddConsumer<LoadTagHistoryCommandConsumer, ILoadTagHistoryCommand>(
                ReceiverApp.Name,
                configurator =>
                {
                    configurator.PrefetchCount = 16;
                    configurator.AutoDelete = true;
                    configurator.Durable = false;
                })
            .AddConsumer<LoadNoteHistoryCommandConsumer, ILoadNoteHistoryCommand>(
                ReceiverApp.Name,
                configurator =>
                {
                    configurator.PrefetchCount = 16;
                    configurator.AutoDelete = true;
                    configurator.Durable = false;
                })
            .AddFireAndForget<IUpdateMetadataCommand>(Lilin.MessageContracts.ReceiverApp.Name)
            .AddFireAndForget<ISearchCompleteCommand>(Meido.MessageContracts.ReceiverApp.Name)
            .AddFireAndForget<INotesUpdatedCommand>(Meido.MessageContracts.ReceiverApp.Name)
            .AddFireAndForget<ITagsUpdatedCommand>(Meido.MessageContracts.ReceiverApp.Name);
}