using ImoutoRebirth.Arachne.Core.Models;

namespace ImoutoRebirth.Arachne.Core.InfrastructureContracts
{
    public interface ISearchEngineProvider
    {
        ISearchEngine Get(SearchEngineType searchEngineType);

        IReadOnlyCollection<ISearchEngine> GetAll();
    }
}