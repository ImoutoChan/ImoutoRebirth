using ImoutoRebirth.Common.Cqrs.Abstract;
using ImoutoRebirth.Common.Domain;
using ImoutoRebirth.Room.Application.Services;

namespace ImoutoRebirth.Room.Application.Cqrs.FoldersSlice;

public record UpdateSourceFolderCommand(
    Guid CollectionId,
    Guid SourceFolderId,
    string Path,
    bool ShouldCheckFormat,
    bool ShouldCheckHashFromName,
    bool ShouldCreateTagsFromSubfolders,
    bool ShouldAddTagFromFilename,
    IReadOnlyCollection<string> SupportedExtensions) : ICommand;
    
internal class UpdateSourceFolderCommandHandler : ICommandHandler<UpdateSourceFolderCommand>
{
    private readonly ICollectionRepository _collectionRepository;

    public UpdateSourceFolderCommandHandler(ICollectionRepository collectionFileRepository) 
        => _collectionRepository = collectionFileRepository;

    public async Task Handle(UpdateSourceFolderCommand request, CancellationToken cancellationToken)
    {
        var (collectionId, sourceFolderId, path, shouldCheckFormat, shouldCheckHashFromName, 
            shouldCreateTagsFromSubfolders, shouldAddTagFromFilename, supportedExtensions) = request;
        var collection = await _collectionRepository.GetById(collectionId).GetAggregateOrThrow();

        collection.UpdateSourceFolder(
            sourceFolderId,
            path,
            shouldCheckFormat,
            shouldCheckHashFromName,
            shouldCreateTagsFromSubfolders,
            shouldAddTagFromFilename,
            supportedExtensions);

        await _collectionRepository.Update(collection);
    }
}
