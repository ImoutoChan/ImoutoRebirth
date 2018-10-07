using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using ImoutoRebirth.Room.DataAccess.Models;
using ImoutoRebirth.Room.Database;
using Microsoft.EntityFrameworkCore;

namespace ImoutoRebirth.Room.DataAccess.Repositories
{
    public class CollectionRepository : ICollectionRepository
    {
        private readonly RoomDbContext _roomDbContext;
        private readonly IMapper _mapper;
        private readonly ICollectionsCacheStorage _collectionsCacheStorage;

        public CollectionRepository(
            RoomDbContext roomDbContext,
            IMapper mapper,
            ICollectionsCacheStorage collectionsCacheStorage)
        {
            _roomDbContext = roomDbContext;
            _mapper = mapper;
            _collectionsCacheStorage = collectionsCacheStorage;
        }

        public async Task<IReadOnlyCollection<OverseedColleciton>> GetAllOverseedCollecitons()
        {
            if (_collectionsCacheStorage.Filled)
                return _collectionsCacheStorage.Collections;

            var collections 
                = await _roomDbContext
                   .Collections
                   .Include(x => x.DestinationFolder)
                   .Include(x => x.SourceFolders)
                   .Include(x => x.Files)
                   .ToListAsync();
            
            var result = collections
                        .Select(x => new OverseedColleciton(
                            _mapper.Map<Collection>(x),
                            _mapper.Map<IReadOnlyCollection<SourceFolder>>(x.SourceFolders),
                            new HashSet<string>(x.Files.Select(y => y.Path)),
                            _mapper.Map<DestinationFolder>(x.DestinationFolder)))
                        .ToArray();

            _collectionsCacheStorage.Fill(result);

            return _collectionsCacheStorage.Collections;
        }
    }
}
