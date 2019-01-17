using System.Threading;
using System.Threading.Tasks;
using ImoutoRebirth.Arachne.Service.Commands;

namespace ImoutoRebirth.Arachne.Service
{
    public interface IRemoteCommandService
    {
        Task SendCommand<T>(UpdateMetadataCommand command, CancellationToken cancellationToken = default);
    }
}