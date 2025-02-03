using ImoutoRebirth.Arachne.MessageContracts;
using MassTransit;
using SearchEngineType = ImoutoRebirth.Arachne.Core.Models.SearchEngineType;

namespace ImoutoRebirth.Arachne.Service.Consumers;

public class Rule34SearchMetadataCommandConsumer : IConsumer<Rule34SearchMetadataCommand>
{
    private readonly ISearchMetadataCommandHandler _searchMetadataCommandHandler;

    public Rule34SearchMetadataCommandConsumer(ISearchMetadataCommandHandler searchMetadataCommandHandler) 
        => _searchMetadataCommandHandler = searchMetadataCommandHandler;

    public async Task Consume(ConsumeContext<Rule34SearchMetadataCommand> context)
        => await _searchMetadataCommandHandler.Search(context, SearchEngineType.Rule34);
}
