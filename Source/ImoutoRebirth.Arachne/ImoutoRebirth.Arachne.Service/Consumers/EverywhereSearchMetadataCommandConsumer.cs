using ImoutoRebirth.Arachne.MessageContracts;
using MassTransit;
using SearchEngineType = ImoutoRebirth.Arachne.Core.Models.SearchEngineType;

namespace ImoutoRebirth.Arachne.Service.Consumers;

public class EverywhereSearchMetadataCommandConsumer : IConsumer<EverywhereSearchMetadataCommand>
{
    private readonly ISearchMetadataCommandHandler _searchMetadataCommandHandler;

    public EverywhereSearchMetadataCommandConsumer(ISearchMetadataCommandHandler searchMetadataCommandHandler)
        => _searchMetadataCommandHandler = searchMetadataCommandHandler;

    public async Task Consume(ConsumeContext<EverywhereSearchMetadataCommand> context)
    {
        foreach (var searchEngineType in Enum.GetValues(typeof(SearchEngineType)).Cast<SearchEngineType>())
            await _searchMetadataCommandHandler.Search(context, searchEngineType);
    }
}
