using ImoutoRebirth.Arachne.MessageContracts;
using MassTransit;
using SearchEngineType = ImoutoRebirth.Arachne.Core.Models.SearchEngineType;

namespace ImoutoRebirth.Arachne.Service.Consumers;

public class SankakuSearchMetadataCommandConsumer : IConsumer<SankakuSearchMetadataCommand>
{
    private readonly ISearchMetadataCommandHandler _searchMetadataCommandHandler;

    public SankakuSearchMetadataCommandConsumer(ISearchMetadataCommandHandler searchMetadataCommandHandler)
        => _searchMetadataCommandHandler = searchMetadataCommandHandler;

    public async Task Consume(ConsumeContext<SankakuSearchMetadataCommand> context)
        => await _searchMetadataCommandHandler.Search(context, SearchEngineType.Sankaku);
}
