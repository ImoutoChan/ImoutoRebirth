using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ImoutoRebirth.Common;
using ImoutoRebirth.Shop.Core.InfrastructureContracts;
using ImoutoRebirth.Shop.Core.Models;

namespace ImoutoRebirth.Shop.Core
{
    /// <summary>
    ///     Arachne service performs search for meta information in specified source.
    /// </summary>
    public class Arachne
    {
        private readonly ISearchEngineProvider _searchEngineProvider;

        public Arachne(ISearchEngineProvider searchEngineProvider)
        {
            _searchEngineProvider = searchEngineProvider;
        }

        public async Task<IReadOnlyCollection<SearchResult>> SearchInAllEngines(Image searchFor) 
            => await _searchEngineProvider
                    .GetAll()
                    .Select(x => x.Search(searchFor))
                    .ToArrayAsync();

        public Task<SearchResult> Search(Image searchFor, SearchEngineType searchEngineType) 
            => _searchEngineProvider.Get(searchEngineType).Search(searchFor);
    }
}