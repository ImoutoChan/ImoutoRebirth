using System.Collections.Generic;
using System.Threading.Tasks;
using ImoutoRebirth.Arachne.Core.Models;

namespace ImoutoRebirth.Arachne.Core
{
    public interface IArachneSearchService
    {
        Task<SearchResult> Search(Image searchFor, SearchEngineType searchEngineType);
        Task<IReadOnlyCollection<SearchResult>> SearchInAllEngines(Image searchFor);
    }
}