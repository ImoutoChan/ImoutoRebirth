using AutoMapper;
using ImoutoRebirth.Room.DataAccess.Exceptions;
using ImoutoRebirth.Room.DataAccess.Models;
using ImoutoRebirth.Room.DataAccess.Repositories.Abstract;
using ImoutoRebirth.Room.Database;
using ImoutoRebirth.Room.Database.Entities;
using Microsoft.EntityFrameworkCore;

namespace ImoutoRebirth.Room.DataAccess.Repositories
{
    public class CollectionRepository : ICollectionRepository
    {
        private readonly RoomDbContext _roomDbContext;
        private readonly IMapper _mapper;

        public CollectionRepository(
            RoomDbContext roomDbContext,
            IMapper mapper)
        {
            _roomDbContext = roomDbContext;
            _mapper = mapper;
        }

        public async Task<IReadOnlyCollection<OversawCollection>> GetAllOversaw()
        {
            var collections 
                = await _roomDbContext
                   .Collections
                   .Include(x => x.DestinationFolder)
                   .Include(x => x.SourceFolders)
                   .ToListAsync();
            
            return collections
                    .Select(x => new OversawCollection(
                        _mapper.Map<Collection>(x),
                        _mapper.Map<IReadOnlyCollection<SourceFolder>>(x.SourceFolders),
                        x.DestinationFolder is null 
                            ? (DestinationFolder) new DefaultDestinationFolder() 
                            : _mapper.Map<CustomDestinationFolder>(x.DestinationFolder)))
                    .ToArray();
        }

        public async Task<IReadOnlyCollection<Collection>> GetAll()
        {
            var collections = await _roomDbContext.Collections.ToListAsync();

            return _mapper.Map<IReadOnlyCollection<Collection>>(collections);
        }

        public async Task<Collection> Add(CollectionCreateData collectionCreateData)
        {
            var newCollection = _mapper.Map<Collection>(collectionCreateData);

            var newCollectionEntity = _mapper.Map<CollectionEntity>(newCollection);
            await _roomDbContext.Collections.AddAsync(newCollectionEntity);
            await _roomDbContext.SaveChangesAsync();

            return newCollection;
        }

        public async Task Remove(Guid id)
        {
            var collection = await _roomDbContext.Collections.FirstOrDefaultAsync(x => x.Id == id);

            if (collection == null)
                throw new EntityNotFoundException<CollectionEntity>(id);

            _roomDbContext.Remove(collection);

            await _roomDbContext.SaveChangesAsync();
        }

        public async Task Rename(Guid id, string newName)
        {
            var collection = await _roomDbContext.Collections.FirstOrDefaultAsync(x => x.Id == id);

            if (collection == null)
                throw new EntityNotFoundException<CollectionEntity>(id);

            collection.Name = newName;

            await _roomDbContext.SaveChangesAsync();
        }
    }
}
