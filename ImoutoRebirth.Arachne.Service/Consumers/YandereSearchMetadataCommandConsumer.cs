using System.Threading.Tasks;
using ImoutoRebirth.Arachne.Core.Models;
using ImoutoRebirth.Arachne.MessageContracts.Commands;
using MassTransit;

namespace ImoutoRebirth.Arachne.Service.Consumers
{
    public class YandereSearchMetadataCommandConsumer : IConsumer<IYandereSearchMetadataCommand>
    {
        private readonly ISearchMetadataCommandHandler _searchMetadataCommandHandler;

        public YandereSearchMetadataCommandConsumer(ISearchMetadataCommandHandler searchMetadataCommandHandler)
        {
            _searchMetadataCommandHandler = searchMetadataCommandHandler;
        }
        
        public async Task Consume(ConsumeContext<IYandereSearchMetadataCommand> context)
        {
            await _searchMetadataCommandHandler.Search(context, SearchEngineType.Yandere);
        }
    }
}