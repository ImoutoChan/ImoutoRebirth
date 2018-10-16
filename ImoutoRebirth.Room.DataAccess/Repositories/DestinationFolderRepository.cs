using System;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using ImoutoRebirth.Room.DataAccess.Exceptions;
using ImoutoRebirth.Room.DataAccess.Models;
using ImoutoRebirth.Room.DataAccess.Repositories.Abstract;
using ImoutoRebirth.Room.Database;
using ImoutoRebirth.Room.Database.Entities;
using Microsoft.EntityFrameworkCore;

namespace ImoutoRebirth.Room.DataAccess.Repositories
{
    public class DestinationFolderRepository : IDestinationFolderRepository
    {
        private readonly RoomDbContext _roomDbContext;
        private readonly IMapper       _mapper;

        public DestinationFolderRepository(RoomDbContext roomDbContext,
                                           IMapper mapper)
        {
            _roomDbContext = roomDbContext;
            _mapper = mapper;
        }

        public async Task<DestinationFolder> Get(Guid collectionGuid)
        {
            var destinationFolders = await _roomDbContext
                                           .DestinationFolders
                                           .Where(x => x.CollectionId == collectionGuid)
                                           .FirstOrDefaultAsync();

            return _mapper.Map<DestinationFolder>(destinationFolders);
        }

        public async Task<DestinationFolder> AddOrReplace(DestinationFolderCreateData createData)
        {
            var destinationFolder = _mapper.Map<CustomDestinationFolder>(createData);
            var entity = _mapper.Map<DestinationFolderEntity>(destinationFolder);

            var currentDestFolderEntity = await _roomDbContext
                                    .DestinationFolders
                                    .FirstOrDefaultAsync(x => x.CollectionId == entity.CollectionId);

            if (currentDestFolderEntity != null)
                _roomDbContext.Remove(currentDestFolderEntity);            

            await _roomDbContext.AddAsync(entity);
            await _roomDbContext.SaveChangesAsync();

            return destinationFolder;
        }

        public async Task Remove(Guid id)
        {
            var folderEntity = await _roomDbContext.DestinationFolders.FirstOrDefaultAsync(x => x.Id == id);

            if (folderEntity == null)
                throw new EntityNotFoundException<DestinationFolderEntity>(id);

            _roomDbContext.Remove(folderEntity);

            await _roomDbContext.SaveChangesAsync();
        }
    }
}