#nullable enable
using ImoutoRebirth.RoomService.WebApi.Client;

namespace ImoutoRebirth.Navigator.Services.Collections;

internal class DestinationFolderService : IDestinationFolderService
{
    private readonly CollectionsClient _collectionsClient;

    public DestinationFolderService(CollectionsClient collectionsClient) => _collectionsClient = collectionsClient;

    public async Task<DestinationFolder?> GetDestinationFolderAsync(Guid collectionId)
    {
        try
        {
            var result = await _collectionsClient.GetDestinationFolderAsync(collectionId);
            return result != null
                ? new DestinationFolder(
                    result.Id,
                    result.CollectionId,
                    result.Path,
                    result.ShouldCreateSubfoldersByHash,
                    result.ShouldRenameByHash,
                    result.FormatErrorSubfolder,
                    result.HashErrorSubfolder,
                    result.WithoutHashErrorSubfolder)
                : null;
        }
        catch (WebApiException e) when (e.StatusCode == 404)
        {
            return null;
        }
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
