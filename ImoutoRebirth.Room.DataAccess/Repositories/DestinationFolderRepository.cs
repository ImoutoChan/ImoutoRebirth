using AutoMapper;
using ImoutoRebirth.Room.DataAccess.Exceptions;
using ImoutoRebirth.Room.DataAccess.Mappers;
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

        public async Task<CustomDestinationFolder?> Get(Guid collectionGuid)
        {
            var destinationFolders = await _roomDbContext
                                           .DestinationFolders
                                           .Where(x => x.CollectionId == collectionGuid)
                                           .FirstOrDefaultAsync();

            return destinationFolders == null
                ? null
                : _mapper.Map<CustomDestinationFolder>(destinationFolders);
        }

        public async Task<CustomDestinationFolder> AddOrReplace(DestinationFolderCreateData createData)
        {
            var currentDestFolderEntity = await _roomDbContext
                                    .DestinationFolders
                                    .FirstOrDefaultAsync(x => x.CollectionId == createData.CollectionId);

            DestinationFolderEntity entity;
            if (currentDestFolderEntity != null)
            {
                entity = createData.ToEntity(currentDestFolderEntity);
            }
            else
            {
                entity = createData.ToEntity();
                await _roomDbContext.AddAsync(entity);
            }

            await _roomDbContext.SaveChangesAsync();

            return _mapper.Map<CustomDestinationFolder>(entity);
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
