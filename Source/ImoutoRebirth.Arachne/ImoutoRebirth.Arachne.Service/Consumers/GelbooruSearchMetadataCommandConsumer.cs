using ImoutoRebirth.Arachne.MessageContracts;
using MassTransit;
using SearchEngineType = ImoutoRebirth.Arachne.Core.Models.SearchEngineType;

namespace ImoutoRebirth.Arachne.Service.Consumers;

public class GelbooruSearchMetadataCommandConsumer : IConsumer<GelbooruSearchMetadataCommand>
{
    private readonly ISearchMetadataCommandHandler _searchMetadataCommandHandler;

    public GelbooruSearchMetadataCommandConsumer(ISearchMetadataCommandHandler searchMetadataCommandHandler)
        => _searchMetadataCommandHandler = searchMetadataCommandHandler;

    public async Task Consume(ConsumeContext<GelbooruSearchMetadataCommand> context)
        => await _searchMetadataCommandHandler.Search(context, SearchEngineType.Gelbooru);
}
