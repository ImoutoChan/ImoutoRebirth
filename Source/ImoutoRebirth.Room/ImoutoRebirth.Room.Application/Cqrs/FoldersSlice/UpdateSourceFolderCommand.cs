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
    IReadOnlyCollection<string> SupportedExtensions,
    bool IsWebhookUploadEnabled,
    string? WebhookUploadUrl) : ICommand;
    
internal class UpdateSourceFolderCommandHandler : ICommandHandler<UpdateSourceFolderCommand>
{
    private readonly ICollectionRepository _collectionRepository;
    private readonly IEventStorage _eventStorage;

    public UpdateSourceFolderCommandHandler(ICollectionRepository collectionFileRepository, IEventStorage eventStorage)
    {
        _collectionRepository = collectionFileRepository;
        _eventStorage = eventStorage;
    }

    public async Task Handle(UpdateSourceFolderCommand request, CancellationToken cancellationToken)
    {
        var (collectionId, sourceFolderId, path, shouldCheckFormat, shouldCheckHashFromName, 
            shouldCreateTagsFromSubfolders, shouldAddTagFromFilename, supportedExtensions, 
            isWebhookUploadEnabled, webhookUploadUrl) = request;
        var collection = await _collectionRepository.GetById(collectionId).GetAggregateOrThrow();

        var result = collection.UpdateSourceFolder(
            sourceFolderId,
            path,
            shouldCheckFormat,
            shouldCheckHashFromName,
            shouldCreateTagsFromSubfolders,
            shouldAddTagFromFilename,
            supportedExtensions,
            isWebhookUploadEnabled,
            webhookUploadUrl);

        _eventStorage.AddRange(result.EventsCollection);
        await _collectionRepository.Update(collection);
    }
}
