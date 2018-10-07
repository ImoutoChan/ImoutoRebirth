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

        public CollectionFileRepository(
            RoomDbContext roomDbContext,
            IMapper mapper)
        {
            _roomDbContext = roomDbContext;
            _mapper = mapper;
        }

        public Task Add(CollectionFile collectionFile)
        {
            var entity = _mapper.Map<CollectionFileEntity>(collectionFile);

            return _roomDbContext.CollectionFiles.AddAsync(entity);
        }
    }
}
