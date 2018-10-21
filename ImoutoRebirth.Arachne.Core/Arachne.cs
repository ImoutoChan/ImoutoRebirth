using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ImoutoRebirth.Arachne.Core.InfrastructureContracts;
using ImoutoRebirth.Arachne.Core.Models;
using ImoutoRebirth.Common;

namespace ImoutoRebirth.Arachne.Core
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