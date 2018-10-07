using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using ImoutoRebirth.Room.DataAccess.Models;
using ImoutoRebirth.Room.Database;
using ImoutoRebirth.Room.Database.Entities;

namespace ImoutoRebirth.Room.DataAccess
{
    public class CollectionFileRepository : ICollectionFileRepository
    {
        private readonly RoomDbContext _roomDbContext;
        private readonly IMapper _mapper;
        private readonly ICollectionsCacheStorage _collectionsCacheStorage;

        public CollectionFileRepository(
            RoomDbContext roomDbContext,
            IMapper mapper,
            ICollectionsCacheStorage collectionsCacheStorage)
        {
            _roomDbContext = roomDbContext;
            _mapper = mapper;
            _collectionsCacheStorage = collectionsCacheStorage;
        }

        public Task Add(CollectionFile collectionFile)
        {
            var entity = _mapper.Map<CollectionFileEntity>(collectionFile);

            _collectionsCacheStorage.Update(list =>
                {
                    list.First(x => x.Collection.Id == collectionFile.CollectionId)
                        .ExistedFiles.Add(collectionFile.Path);
                });

            return _roomDbContext.CollectionFiles.AddAsync(entity);
        }
    }
}
