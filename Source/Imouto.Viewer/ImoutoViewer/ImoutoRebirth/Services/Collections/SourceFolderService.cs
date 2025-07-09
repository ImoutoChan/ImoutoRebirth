using ImoutoRebirth.Room.WebApi.Client;

namespace ImoutoViewer.ImoutoRebirth.Services.Collections;

internal class SourceFolderService : ISourceFolderService
{
    private readonly CollectionsClient _collectionsClient;

    public SourceFolderService(CollectionsClient collectionsClient) => _collectionsClient = collectionsClient;

    public async Task<IReadOnlyCollection<SourceFolder>> GetSourceFoldersAsync(Guid collectionId)
    {
        var result = await _collectionsClient.GetSourceFoldersAsync(collectionId);

        return result
            .Select(x => new SourceFolder(
                Id: x.Id,
                CollectionId: x.CollectionId,
                Path: x.Path!,
                ShouldCheckFormat: x.ShouldCheckFormat,
                ShouldCheckHashFromName: x.ShouldCheckHashFromName,
                ShouldCreateTagsFromSubfolders: x.ShouldCreateTagsFromSubfolders,
                ShouldAddTagFromFilename: x.ShouldAddTagFromFilename,
                SupportedExtensions: x.SupportedExtensions ?? Array.Empty<string>(),
                IsWebhookUploadEnabled: x.IsWebhookUploadEnabled,
                WebhookUploadUrl: x.WebhookUploadUrl
            ))
            .ToArray();
    }

    public async Task<SourceFolder> AddSourceFolderAsync(SourceFolder sourceFolder)
    {
        var id = await _collectionsClient.AddSourceFolderAsync(new(
            sourceFolder.CollectionId,
            sourceFolder.IsWebhookUploadEnabled,
            sourceFolder.Path,
            sourceFolder.ShouldAddTagFromFilename,
            sourceFolder.ShouldCheckFormat,
            sourceFolder.ShouldCheckHashFromName,
            sourceFolder.ShouldCreateTagsFromSubfolders,
            sourceFolder.SupportedExtensions,
            sourceFolder.WebhookUploadUrl));

        return sourceFolder with { Id = id };
    }

    public async Task UpdateSourceFolderAsync(SourceFolder sourceFolder)
    {
        if (!sourceFolder.Id.HasValue)
            throw new ArgumentException(nameof(sourceFolder));

        await _collectionsClient.UpdateSourceFolderAsync(new(
            sourceFolder.CollectionId,
            sourceFolder.IsWebhookUploadEnabled,
            sourceFolder.Path,
            sourceFolder.ShouldAddTagFromFilename,
            sourceFolder.ShouldCheckFormat,
            sourceFolder.ShouldCheckHashFromName,
            sourceFolder.ShouldCreateTagsFromSubfolders,
            sourceFolder.Id.Value,
            sourceFolder.SupportedExtensions,
            sourceFolder.WebhookUploadUrl));
    }

    public async Task DeleteSourceFolderAsync(Guid collectionId, Guid sourceFolderId) 
        => await _collectionsClient.DeleteSourceFolderAsync(collectionId, sourceFolderId);
}
