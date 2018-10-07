using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using ImoutoRebirth.Room.DataAccess.Models;
using ImoutoRebirth.Room.Database;
using Microsoft.EntityFrameworkCore;

namespace ImoutoRebirth.Room.DataAccess
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

        public async Task<IReadOnlyCollection<OverseedColleciton>> GetAllOverseedCollecitons()
        {
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

            return result;
        }
    }
}
