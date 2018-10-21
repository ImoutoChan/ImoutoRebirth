using System.Collections.Generic;
using ImoutoRebirth.Shop.Core.Models;

namespace ImoutoRebirth.Shop.Core.InfrastructureContracts
{
    public interface ISearchEngineProvider
    {
        ISearchEngine Get(SearchEngineType searchEngineType);

        IReadOnlyCollection<ISearchEngine> GetAll();
    }
}