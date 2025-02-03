using ImoutoRebirth.Arachne.MessageContracts;
using MassTransit;
using SearchEngineType = ImoutoRebirth.Arachne.Core.Models.SearchEngineType;

namespace ImoutoRebirth.Arachne.Service.Consumers;

public class ExHentaiSearchMetadataCommandConsumer : IConsumer<ExHentaiSearchMetadataCommand>
{
    private readonly ISearchMetadataCommandHandler _searchMetadataCommandHandler;

    public ExHentaiSearchMetadataCommandConsumer(ISearchMetadataCommandHandler searchMetadataCommandHandler)
        => _searchMetadataCommandHandler = searchMetadataCommandHandler;

    public async Task Consume(ConsumeContext<ExHentaiSearchMetadataCommand> context)
        => await _searchMetadataCommandHandler.Search(context, SearchEngineType.ExHentai);
}
