using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using ImoutoRebirth.Room.DataAccess.Exceptions;
using ImoutoRebirth.Room.DataAccess.Models;
using ImoutoRebirth.Room.Database;
using ImoutoRebirth.Room.Database.Entities;
using Microsoft.EntityFrameworkCore;

namespace ImoutoRebirth.Room.DataAccess.Repositories.Abstract
{
    public class SourceFolderRepository : ISourceFolderRepository
    {
        private readonly RoomDbContext _roomDbContext;
        private readonly IMapper       _mapper;

        public SourceFolderRepository(RoomDbContext roomDbContext,
            IMapper mapper)
        {
            _roomDbContext = roomDbContext;
            _mapper = mapper;
        }

        public async Task<IReadOnlyCollection<SourceFolder>> Get(Guid collectionGuid)
        {
            var sourceFolders = await _roomDbContext
                                     .SourceFolders
                                     .Where(x => x.CollectionId == collectionGuid)
                                     .ToArrayAsync();

            return _mapper.Map<SourceFolder[]>(sourceFolders);
        }

        public async Task<SourceFolder> Add(SourceFolderCreateData createData)
        {
            var sourceFolder = _mapper.Map<SourceFolder>(createData);

            var entity = _mapper.Map<SourceFolderEntity>(sourceFolder);
            await _roomDbContext.AddAsync(entity);
            await _roomDbContext.SaveChangesAsync();

            return sourceFolder;
        }

        public async Task Remove(Guid id)
        {
            var folderEntity = await _roomDbContext.SourceFolders.FirstOrDefaultAsync(x => x.Id == id);

            if (folderEntity == null)
                throw new EntityNotFoundException<SourceFolderEntity>(id);

            _roomDbContext.Remove(folderEntity);

            await _roomDbContext.SaveChangesAsync();
        }
    }
}