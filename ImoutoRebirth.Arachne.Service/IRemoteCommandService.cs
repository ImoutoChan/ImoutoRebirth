using System.Threading;
using System.Threading.Tasks;

namespace ImoutoRebirth.Arachne.Service
{
    public interface IRemoteCommandService
    {
        Task SendCommand<T>(T command, CancellationToken cancellationToken = default) where T : class;
    }
}