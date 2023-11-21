using ImoutoRebirth.Common.Cqrs.Abstract;
using ImoutoRebirth.Common.Domain;
using ImoutoRebirth.Room.Application.Services;

namespace ImoutoRebirth.Room.Application.Cqrs.FoldersSlice;

public record AddSourceFolderCommand(
    Guid CollectionId,
    string Path,
    bool ShouldCheckFormat,
    bool ShouldCheckHashFromName,
    bool ShouldCreateTagsFromSubfolders,
    bool ShouldAddTagFromFilename,
    IReadOnlyCollection<string> SupportedExtensions) : ICommand<Guid>;

internal class AddSourceFolderCommandHandler : ICommandHandler<AddSourceFolderCommand, Guid>
{
    private readonly ICollectionRepository _collectionRepository;

    public AddSourceFolderCommandHandler(ICollectionRepository collectionFileRepository) 
        => _collectionRepository = collectionFileRepository;

    public async Task<Guid> Handle(AddSourceFolderCommand request, CancellationToken cancellationToken)
    {
        var (collectionId, path, shouldCheckFormat, shouldCheckHashFromName, shouldCreateTagsFromSubfolders,
            shouldAddTagFromFilename, supportedExtensions) = request;
        var collection = await _collectionRepository.GetById(collectionId).GetAggregateOrThrow();

        var id = collection.AddSourceFolder(
            path,
            shouldCheckFormat,
            shouldCheckHashFromName,
            shouldCreateTagsFromSubfolders,
            shouldAddTagFromFilename,
            supportedExtensions);

        await _collectionRepository.Update(collection);

        return id;
    }
}
