using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using ImoutoRebirth.Room.WebApi.Client;
using ImoutoRebirth.Room.WebApi.Client.Models;

namespace ImoutoRebirth.Navigator.Services.Collections;

internal class CollectionService : ICollectionService
{
    private readonly ICollections _collectionsHttpService;
    private readonly IMapper _mapper;

    public CollectionService(ICollections collectionsHttpService, IMapper mapper)
    {
        _collectionsHttpService = collectionsHttpService;
        _mapper = mapper;
    }

    public async Task<IReadOnlyCollection<Collection>> GetAllCollectionsAsync()
    {
        var result = await _collectionsHttpService.GetAllAsync();

        return result.Select(x => new Collection(x.Id, x.Name)).ToArray();
    }

    public async Task<Collection> CreateCollectionAsync(string name)
    {
        var result = await _collectionsHttpService.CreateAsync(new CollectionCreateRequest(name));

        return _mapper.Map<Collection>(result);
    }

    public Task RenameCollection(Guid collectionId, string name)
    {
        return _collectionsHttpService.RenameAsync(collectionId, name);
    }

    public async Task DeleteCollectionAsync(Guid guid)
    {
        await _collectionsHttpService.DeleteAsync(guid);
    }
}