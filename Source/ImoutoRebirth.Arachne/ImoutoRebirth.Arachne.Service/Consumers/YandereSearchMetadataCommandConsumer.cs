using ImoutoRebirth.Arachne.MessageContracts;
using MassTransit;
using SearchEngineType = ImoutoRebirth.Arachne.Core.Models.SearchEngineType;

namespace ImoutoRebirth.Arachne.Service.Consumers;

public class YandereSearchMetadataCommandConsumer : IConsumer<YandereSearchMetadataCommand>
{
    private readonly ISearchMetadataCommandHandler _searchMetadataCommandHandler;

    public YandereSearchMetadataCommandConsumer(ISearchMetadataCommandHandler searchMetadataCommandHandler)
        => _searchMetadataCommandHandler = searchMetadataCommandHandler;

    public async Task Consume(ConsumeContext<YandereSearchMetadataCommand> context)
        => await _searchMetadataCommandHandler.Search(context, SearchEngineType.Yandere);
}
