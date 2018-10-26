using System;
using System.Threading.Tasks;
using ImoutoRebirth.Arachne.Core;
using ImoutoRebirth.Arachne.Core.Models;
using ImoutoRebirth.Arachne.MessageContracts;
using MassTransit;

namespace ImoutoRebirth.Arachne.Service
{
    public class SearchMetadataCommandHandler : ISearchMetadataCommandHandler
    {
        private readonly IArachneSearchService _arachneSearchService;

        public SearchMetadataCommandHandler(IArachneSearchService arachneSearchService)
        {
            _arachneSearchService = arachneSearchService;
        }

        public async Task Search(ConsumeContext<ISearchMetadataCommand> context, SearchEngineType where)
        {
            var md5 = context.Message.Md5;

            var searchResults = await _arachneSearchService.Search(new Image(md5), where);

            // todo sendSearchResults

            if (searchResults is Metadata searchResult)
                Console.Out.WriteLine(searchResult.Source + " | " + searchResult.IsFound);
        }
    }
}