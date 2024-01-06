using ImoutoRebirth.Room.WebApi.Client;

namespace ImoutoRebirth.Harpy.Services.SaveFavorites.Services.Room;

internal class RoomSavedChecker
{
    private readonly CollectionFilesClient _collectionFilesClient;

    public RoomSavedChecker(CollectionFilesClient collectionFilesClient) 
        => _collectionFilesClient = collectionFilesClient;

    public async Task<IReadOnlyCollection<Post>> GetOnlyNewPosts(IReadOnlyCollection<Post> allPosts)
    {
        var results = await _collectionFilesClient.SearchCollectionFilesAsync(
            new(null, null, null, allPosts.Select(x => x.Md5).ToArray(), null, null));

        var existed = results.Select(x => x.Md5).ToHashSet();
        return allPosts.Where(x => !existed.Contains(x.Md5)).ToArray();
    }
}
