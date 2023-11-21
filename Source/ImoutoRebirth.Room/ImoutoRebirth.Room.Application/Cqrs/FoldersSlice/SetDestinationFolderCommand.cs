using ImoutoRebirth.Common.Cqrs.Abstract;
using ImoutoRebirth.Common.Domain;
using ImoutoRebirth.Room.Application.Services;

namespace ImoutoRebirth.Room.Application.Cqrs.FoldersSlice;

public record SetDestinationFolderCommand(
    Guid CollectionId,
    string Path,
    bool ShouldCreateSubfoldersByHash,
    bool ShouldRenameByHash,
    string FormatErrorSubfolder,
    string HashErrorSubfolder,
    string WithoutHashErrorSubfolder) : ICommand<Guid>;

internal class SetDestinationFolderCommandHandler : ICommandHandler<SetDestinationFolderCommand, Guid>
{
    private readonly ICollectionRepository _collectionRepository;

    public SetDestinationFolderCommandHandler(ICollectionRepository collectionFileRepository) 
        => _collectionRepository = collectionFileRepository;

    public async Task<Guid> Handle(SetDestinationFolderCommand request, CancellationToken cancellationToken)
    {
        var (collectionId, path, shouldCreateSubfoldersByHash, shouldRenameByHash, formatErrorSubfolder,
            hashErrorSubfolder, withoutHashErrorSubfolder) = request;
        var collection = await _collectionRepository.GetById(collectionId).GetAggregateOrThrow();

        var newId = collection.SetDestinationFolder(
            path,
            shouldCreateSubfoldersByHash,
            shouldRenameByHash,
            formatErrorSubfolder,
            hashErrorSubfolder,
            withoutHashErrorSubfolder);

        await _collectionRepository.Update(collection);

        return newId;
    }
}
