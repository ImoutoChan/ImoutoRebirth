using ImoutoRebirth.Common;
using ImoutoRebirth.Lilin.MessageContracts;
using ImoutoRebirth.Meido.MessageContracts;
using ImoutoRebirth.Room.Core.Services.Abstract;
using MassTransit;

namespace ImoutoRebirth.Room.Infrastructure.Service;

internal class RemoteCommandService : IRemoteCommandService
{
    private readonly IBus _bus;

    public RemoteCommandService(IBus bus)
    {
        _bus = bus;
    }
        
    public async Task UpdateMetadataRequest(Guid fileId, string md5)
    {
        var command = new
        {
            Md5 = md5,
            FileId = fileId
        };

        await _bus.Send<INewFileCommand>(command);
    }

    public async Task SaveTags(Guid fileId, IReadOnlyCollection<string> tags)
    {
        if (tags.None())
            return;
        
        var command = new
        {
            FileId = fileId,
            MetadataSource = MetadataSource.Manual,
            FileNotes = Array.Empty<IFileNote>(),
            FileTags = tags.Select(x => new
            {
                Type = "Location",
                Name = x
            }).ToArray()
        };

        await _bus.Send<IUpdateMetadataCommand>(command);
    }
}
