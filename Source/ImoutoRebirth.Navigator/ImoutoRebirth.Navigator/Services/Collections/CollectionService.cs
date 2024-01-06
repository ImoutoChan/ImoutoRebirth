using ImoutoRebirth.Room.WebApi.Client;

namespace ImoutoRebirth.Navigator.Services.Collections;

internal class CollectionService : ICollectionService
{
    private readonly CollectionsClient _collectionsClient;

    public CollectionService(CollectionsClient collectionsClient) => _collectionsClient = collectionsClient;

    public async Task<IReadOnlyCollection<Collection>> GetAllCollectionsAsync()
    {
        var result = await _collectionsClient.GetAllCollectionsAsync();
        return result.Select(x => new Collection(x.Id, x.Name)).ToArray();
    }

    public async Task<Collection> CreateCollectionAsync(string name)
    {
        var id = await _collectionsClient.CreateCollectionAsync(new(name));
        return new(id, name);
    }

    public async Task RenameCollection(Guid collectionId, string name) 
        => await _collectionsClient.RenameCollectionAsync(collectionId, name);

    public async Task DeleteCollectionAsync(Guid id) => await _collectionsClient.DeleteCollectionAsync(id);
}
