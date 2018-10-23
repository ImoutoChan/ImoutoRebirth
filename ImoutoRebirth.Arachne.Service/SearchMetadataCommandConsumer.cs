using System.Threading.Tasks;
using ImoutoRebirth.Arachne.Core;
using ImoutoRebirth.Arachne.Core.Models;
using ImoutoRebirth.Arachne.MessageContracts;
using MassTransit;

namespace ImoutoRebirth.Arachne.Service
{
    public class SearchMetadataCommandConsumer : IConsumer<ISearchMetadataCommand>
    {
        private readonly IArachneSearchService _arachneSearchService;

        public SearchMetadataCommandConsumer(IArachneSearchService arachneSearchService)
        {
            _arachneSearchService = arachneSearchService;
        }

        public async Task Consume(ConsumeContext<ISearchMetadataCommand> context)
        {
            var md5 = context.Message.Md5;

            var searchResults = await _arachneSearchService.SearchInAllEngines(new Image(md5));

            // todo sendSearchResults
        }
    }
}
