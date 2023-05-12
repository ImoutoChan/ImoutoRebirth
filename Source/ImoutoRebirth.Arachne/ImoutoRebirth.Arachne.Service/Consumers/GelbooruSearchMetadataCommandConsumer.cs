using ImoutoRebirth.Arachne.Core.Models;
using ImoutoRebirth.Arachne.MessageContracts.Commands;
using MassTransit;

namespace ImoutoRebirth.Arachne.Service.Consumers;

public class GelbooruSearchMetadataCommandConsumer : IConsumer<IGelbooruSearchMetadataCommand>
{
    private readonly ISearchMetadataCommandHandler _searchMetadataCommandHandler;

    public GelbooruSearchMetadataCommandConsumer(ISearchMetadataCommandHandler searchMetadataCommandHandler)
    {
        _searchMetadataCommandHandler = searchMetadataCommandHandler;
    }
        
    public async Task Consume(ConsumeContext<IGelbooruSearchMetadataCommand> context)
    {
        await _searchMetadataCommandHandler.Search(context, SearchEngineType.Gelbooru);
    }
}
