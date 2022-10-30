using AutoMapper;
using ImoutoRebirth.RoomService.WebApi.Client;

namespace ImoutoRebirth.Navigator.Services.Collections;

internal class CollectionService : ICollectionService
{
    private readonly CollectionsClient _collectionsClient;
    private readonly IMapper _mapper;

    public CollectionService(IMapper mapper, CollectionsClient collectionsClient)
    {
        _mapper = mapper;
        _collectionsClient = collectionsClient;
    }

    public async Task<IReadOnlyCollection<Collection>> GetAllCollectionsAsync()
    {
        var result = await _collectionsClient.GetAllAsync();

        return result.Select(x => new Collection(x.Id, x.Name)).ToArray();
    }

    public async Task<Collection> CreateCollectionAsync(string name)
    {
        var result = await _collectionsClient.CreateAsync(new CollectionCreateRequest(name));

        return _mapper.Map<Collection>(result);
    }

    public Task RenameCollection(Guid collectionId, string name)
    {
        return _collectionsClient.RenameAsync(collectionId, name);
    }

    public async Task DeleteCollectionAsync(Guid guid)
    {
        await _collectionsClient.DeleteAsync(guid);
    }
}
