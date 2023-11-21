using AutoMapper;
using ImoutoRebirth.RoomService.WebApi.Client;

namespace ImoutoRebirth.Navigator.Services.Collections;

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
                Path: x.Path,
                ShouldCheckFormat: x.ShouldCheckFormat,
                ShouldCheckHashFromName: x.ShouldCheckHashFromName,
                ShouldCreateTagsFromSubfolders: x.ShouldCreateTagsFromSubfolders,
                ShouldAddTagFromFilename: x.ShouldAddTagFromFilename,
                SupportedExtensions: x.SupportedExtensions
            ))
            .ToArray();
    }

    public async Task<SourceFolder> AddSourceFolderAsync(SourceFolder sourceFolder)
    {
        var id = await _collectionsClient.AddSourceFolderAsync(new(
            sourceFolder.CollectionId,
            sourceFolder.Path,
            sourceFolder.ShouldAddTagFromFilename,
            sourceFolder.ShouldCheckFormat,
            sourceFolder.ShouldCheckHashFromName,
            sourceFolder.ShouldCreateTagsFromSubfolders,
            sourceFolder.SupportedExtensions));

        return sourceFolder with { Id = id };
    }

    public async Task UpdateSourceFolderAsync(SourceFolder sourceFolder)
    {
        if (!sourceFolder.Id.HasValue)
            throw new ArgumentException("Can't update new collection", nameof(sourceFolder));

        await _collectionsClient.UpdateSourceFolderAsync(new(
            sourceFolder.CollectionId,
            sourceFolder.Path,
            sourceFolder.ShouldAddTagFromFilename,
            sourceFolder.ShouldCheckFormat,
            sourceFolder.ShouldCheckHashFromName,
            sourceFolder.ShouldCreateTagsFromSubfolders,
            sourceFolder.Id.Value,
            sourceFolder.SupportedExtensions));
    }

    public async Task DeleteSourceFolderAsync(Guid collectionId, Guid sourceFolderId) 
        => await _collectionsClient.DeleteSourceFolderAsync(collectionId, sourceFolderId);
}
