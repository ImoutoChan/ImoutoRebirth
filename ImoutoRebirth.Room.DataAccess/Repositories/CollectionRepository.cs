using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using ImoutoRebirth.Room.DataAccess.Models;
using ImoutoRebirth.Room.DataAccess.Repositories.Abstract;
using ImoutoRebirth.Room.Database;
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

        public async Task<IReadOnlyCollection<OversawCollection>> GetAllOversawCollections()
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
                        _mapper.Map<DestinationFolder>(x.DestinationFolder)))
                    .ToArray();
        }

        public async Task<IReadOnlyCollection<Collection>> GetCollections()
        {
            var collections = await _roomDbContext.Collections.ToListAsync();

            return _mapper.Map<IReadOnlyCollection<Collection>>(collections);
        }
    }
}
