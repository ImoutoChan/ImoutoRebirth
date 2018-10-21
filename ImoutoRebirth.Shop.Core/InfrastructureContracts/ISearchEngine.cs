using System.Threading.Tasks;
using ImoutoRebirth.Shop.Core.Models;

namespace ImoutoRebirth.Shop.Core.InfrastructureContracts
{
    public interface ISearchEngine
    {
        SearchEngineType SearchEngineType { get; }

        Task<SearchResult> Search(Image image);
    }
}