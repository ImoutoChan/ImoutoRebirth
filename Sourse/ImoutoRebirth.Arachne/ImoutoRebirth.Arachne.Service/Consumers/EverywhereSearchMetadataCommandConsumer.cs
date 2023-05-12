using ImoutoRebirth.Arachne.Core.Models;
using ImoutoRebirth.Arachne.MessageContracts.Commands;
using MassTransit;

namespace ImoutoRebirth.Arachne.Service.Consumers;

public class EverywhereSearchMetadataCommandConsumer : IConsumer<IEverywhereSearchMetadataCommand>
{
    private readonly ISearchMetadataCommandHandler _searchMetadataCommandHandler;

    public EverywhereSearchMetadataCommandConsumer(ISearchMetadataCommandHandler searchMetadataCommandHandler)
    {
        _searchMetadataCommandHandler = searchMetadataCommandHandler;
    }
        
    public async Task Consume(ConsumeContext<IEverywhereSearchMetadataCommand> context)
    {
        foreach (var searchEngineType in Enum.GetValues(typeof(SearchEngineType)).Cast<SearchEngineType>())
            await _searchMetadataCommandHandler.Search(context, searchEngineType);
    }
}