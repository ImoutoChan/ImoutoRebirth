#nullable enable
using ImoutoRebirth.RoomService.WebApi.Client;

namespace ImoutoViewer.ImoutoRebirth.Services.Collections;

internal class DestinationFolderService : IDestinationFolderService
{
    private readonly CollectionsClient _collectionsClient;

    public DestinationFolderService(CollectionsClient collectionsClient) => _collectionsClient = collectionsClient;

    public async Task<DestinationFolder?> GetDestinationFolderAsync(Guid collectionId)
    {
        var optionalResult = await _collectionsClient.GetDestinationFolderAsync(collectionId);
        if (!optionalResult.HasValue)
            return null;
        
        var result = optionalResult.Value!;

        return new DestinationFolder(
            result.Id,
            result.CollectionId,
            result.Path,
            result.ShouldCreateSubfoldersByHash,
            result.ShouldRenameByHash,
            result.FormatErrorSubfolder,
            result.HashErrorSubfolder,
            result.WithoutHashErrorSubfolder);
    }

    public async Task<DestinationFolder> SetDestinationFolderAsync(DestinationFolder destinationFolder)
    {
        var id = await _collectionsClient.SetDestinationFolderAsync(
            new(destinationFolder.CollectionId,
                destinationFolder.FormatErrorSubfolder,
                destinationFolder.HashErrorSubfolder,
                destinationFolder.Path,
                destinationFolder.ShouldCreateSubfoldersByHash,
                destinationFolder.ShouldRenameByHash,
                destinationFolder.WithoutHashErrorSubfolder));

        return destinationFolder with { Id = id };
    }

    public Task DeleteDestinationFolderAsync(Guid collectionId)
        => _collectionsClient.DeleteDestinationFolderAsync(collectionId);
}
