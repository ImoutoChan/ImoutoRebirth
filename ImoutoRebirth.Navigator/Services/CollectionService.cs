using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ImoutoRebirth.Room.WebApi.Client;

namespace ImoutoRebirth.Navigator.Services
{
    class CollectionService : ICollectionService
    {
        private readonly ICollections _collectionsHttpService;

        public CollectionService(ICollections collectionsHttpService)
        {
            _collectionsHttpService = collectionsHttpService;
        }

        public async Task<IReadOnlyCollection<Collection>> GetAllCollectionsAsync()
        {
            var result = await _collectionsHttpService.GetAllAsync();

            return result.Select(x => new Collection(x.Id, x.Name)).ToArray();
        }

        public Task<Collection> CreateCollectionAsync(string name)
        {
            throw new NotImplementedException();
        }

        public Task<Collection> RenameCollection(string name)
        {
            throw new NotImplementedException();
        }

        public Task DeleteCollectionAsync(Guid guid)
        {
            throw new NotImplementedException();
        }
    }
}