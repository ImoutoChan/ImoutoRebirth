using System.Threading.Tasks;

namespace ImoutoRebirth.Meido.Core.SourceActualizingState
{
    public interface ISourceActualizer
    {
        Task RequestActualization();
    }
}