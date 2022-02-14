using ImoutoRebirth.Arachne.Core.Models;
using ImoutoRebirth.Arachne.MessageContracts.Commands;
using MassTransit;

namespace ImoutoRebirth.Arachne.Service.Consumers
{
    public class SankakuSearchMetadataCommandConsumer : IConsumer<ISankakuSearchMetadataCommand>
    {
        private readonly ISearchMetadataCommandHandler _searchMetadataCommandHandler;

        public SankakuSearchMetadataCommandConsumer(ISearchMetadataCommandHandler searchMetadataCommandHandler)
        {
            _searchMetadataCommandHandler = searchMetadataCommandHandler;
        }
        
        public async Task Consume(ConsumeContext<ISankakuSearchMetadataCommand> context)
        {
            await _searchMetadataCommandHandler.Search(context, SearchEngineType.Sankaku);
        }
    }
}