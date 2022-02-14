using ImoutoRebirth.Arachne.Core.Models;
using ImoutoRebirth.Arachne.MessageContracts.Commands;
using MassTransit;

namespace ImoutoRebirth.Arachne.Service.Consumers
{
    public class DanbooruSearchMetadataCommandConsumer : IConsumer<IDanbooruSearchMetadataCommand>
    {
        private readonly ISearchMetadataCommandHandler _searchMetadataCommandHandler;

        public DanbooruSearchMetadataCommandConsumer(ISearchMetadataCommandHandler searchMetadataCommandHandler)
        {
            _searchMetadataCommandHandler = searchMetadataCommandHandler;
        }
        
        public async Task Consume(ConsumeContext<IDanbooruSearchMetadataCommand> context)
        {
            await _searchMetadataCommandHandler.Search(context, SearchEngineType.Danbooru);
        }
    }
}