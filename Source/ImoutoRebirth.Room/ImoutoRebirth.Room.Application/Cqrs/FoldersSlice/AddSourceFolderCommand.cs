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
    IReadOnlyCollection<string> SupportedExtensions,
    bool IsWebhookUploadEnabled,
    string? WebhookUploadUrl) : ICommand<Guid>;

internal class AddSourceFolderCommandHandler : ICommandHandler<AddSourceFolderCommand, Guid>
{
    private readonly ICollectionRepository _collectionRepository;
    private readonly IEventStorage _eventStorage;

    public AddSourceFolderCommandHandler(ICollectionRepository collectionFileRepository, IEventStorage eventStorage)
    {
        _collectionRepository = collectionFileRepository;
        _eventStorage = eventStorage;
    }

    public async Task<Guid> Handle(AddSourceFolderCommand request, CancellationToken cancellationToken)
    {
        var (collectionId, path, shouldCheckFormat, shouldCheckHashFromName, shouldCreateTagsFromSubfolders,
            shouldAddTagFromFilename, supportedExtensions, isWebhookUploadEnabled, webhookUploadUrl) = request;
        var collection = await _collectionRepository.GetById(collectionId).GetAggregateOrThrow();

        var result = collection.AddSourceFolder(
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

        return result.Result;
    }
}
