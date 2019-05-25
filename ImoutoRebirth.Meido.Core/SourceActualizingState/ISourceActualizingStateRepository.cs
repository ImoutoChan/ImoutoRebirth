using System.Threading.Tasks;

namespace ImoutoRebirth.Meido.Core.SourceActualizingState
{
    public interface ISourceActualizingStateRepository
    {
        Task Add(SourceActualizingState parsingStatus);
    }
}