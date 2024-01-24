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

        services.AddSingleton<TagsSearchEngineHistoryAccessor>();
        services.AddSingleton<NotesSearchEngineHistoryAccessor>();

        return services;
    }

    public static MassTransitConfigurator AddArachneMassTransitSetup(
        this MassTransitConfigurator builder)
        => builder
            .AddCommand<IUpdateMetadataCommand>()
            .AddCommand<ISearchCompleteCommand>()
            .AddCommand<INotesUpdatedCommand>()
            .AddCommand<ITagsUpdatedCommand>();
}
