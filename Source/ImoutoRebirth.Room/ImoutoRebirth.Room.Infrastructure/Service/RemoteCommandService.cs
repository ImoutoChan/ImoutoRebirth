using ImoutoRebirth.Common;
using ImoutoRebirth.Lilin.MessageContracts;
using ImoutoRebirth.Meido.MessageContracts;
using ImoutoRebirth.Room.Application.Services;
using MassTransit;

namespace ImoutoRebirth.Room.Infrastructure.Service;

internal class RemoteCommandService : IRemoteCommandService
{
    private readonly IBus _bus;

    public RemoteCommandService(IBus bus) => _bus = bus;

    public async Task UpdateMetadataRequest(Guid fileId, string md5, string fileName)
    {
        var command = new NewFileCommand(md5, fileId, fileName);
        await _bus.Send(command);
    }

    public async Task SaveTags(Guid fileId, IReadOnlyCollection<string> tags)
    {
        if (tags.None())
            return;
        
        var command = new UpdateMetadataCommand(
            fileId,
            MetadataSource.Manual,
            [],
            tags.Select(x => new FileTag("Location", x, null, [])).ToArray());

        await _bus.Send(command);
    }
}
