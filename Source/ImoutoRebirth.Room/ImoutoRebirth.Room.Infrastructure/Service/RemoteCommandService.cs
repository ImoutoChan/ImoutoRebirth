using ImoutoRebirth.Common;
using ImoutoRebirth.Lamia.MessageContracts;
using ImoutoRebirth.Lilin.MessageContracts;
using ImoutoRebirth.Meido.MessageContracts;
using ImoutoRebirth.Room.Application.Services;
using MassTransit;

namespace ImoutoRebirth.Room.Infrastructure.Service;

internal class RemoteCommandService : IRemoteCommandService
{
    private readonly IBus _bus;

    public RemoteCommandService(IBus bus) => _bus = bus;

    public async Task RequestMetadataUpdate(Guid fileId, string md5, string fileName)
    {
        var command = new NewFileCommand(md5, fileId, fileName);
        await _bus.Send(command);
    }

    public async Task RequestMetadataUpdateForRenamedFile(Guid fileId, string md5, string newFileName)
    {
        var command = new ProcessRenamedFileCommand(md5, fileId, newFileName);
        await _bus.Send(command);
    }

    public async Task SaveLocationTags(Guid fileId, IReadOnlyCollection<string> tags)
    {
        if (tags.None())
            return;
        
        var command = new UpdateLocationTagsCommand(fileId, tags.ToArray());
        await _bus.Send(command);
    }

    public async Task UpdateFileMetadataRequest(Guid fileId, string fullName)
    {
        var command = new ExtractFileMetadataCommand(fullName, fileId);
        await _bus.Send(command);
    }
}
