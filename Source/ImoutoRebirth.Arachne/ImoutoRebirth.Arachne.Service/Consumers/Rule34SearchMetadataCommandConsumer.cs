using ImoutoRebirth.Arachne.Core.Models;
using ImoutoRebirth.Arachne.MessageContracts.Commands;
using MassTransit;

namespace ImoutoRebirth.Arachne.Service.Consumers;

public class Rule34SearchMetadataCommandConsumer : IConsumer<IRule34SearchMetadataCommand>
{
    private readonly ISearchMetadataCommandHandler _searchMetadataCommandHandler;

    public Rule34SearchMetadataCommandConsumer(ISearchMetadataCommandHandler searchMetadataCommandHandler) 
        => _searchMetadataCommandHandler = searchMetadataCommandHandler;

    public async Task Consume(ConsumeContext<IRule34SearchMetadataCommand> context)
    {
        await _searchMetadataCommandHandler.Search(context, SearchEngineType.Rule34);
    }
}