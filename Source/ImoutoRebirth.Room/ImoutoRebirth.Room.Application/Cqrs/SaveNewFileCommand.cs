using ImoutoRebirth.Common.Cqrs.Abstract;
using ImoutoRebirth.Room.Domain;

namespace ImoutoRebirth.Room.Application.Cqrs;

public record SaveNewFileCommand(Guid CollectionId, SystemFileMoved MovedFile) : ICommand<Guid>;

internal class SaveNewFileCommandHandler : ICommandHandler<SaveNewFileCommand, Guid>
{
    private readonly ICollectionFileRepository _collectionFileRepository;

    public SaveNewFileCommandHandler(ICollectionFileRepository collectionFileRepository) 
        => _collectionFileRepository = collectionFileRepository;

    public async Task<Guid> Handle(SaveNewFileCommand request, CancellationToken cancellationToken)
    {
        var (collectionId, movedFile) = request;

        var newId = Guid.NewGuid();

        var newFile = new CollectionFile(
            newId,
            collectionId,
            movedFile.MovedFileInfo.FullName,
            movedFile.SystemFile.Md5,
            movedFile.SystemFile.Size,
            movedFile.SystemFile.File.FullName);

        await _collectionFileRepository.Create(newFile);

        return newId;
    }
}
