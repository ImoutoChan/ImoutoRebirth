using AutoMapper;
using ImoutoRebirth.Room.DataAccess.Exceptions;
using ImoutoRebirth.Room.DataAccess.Models;
using ImoutoRebirth.Room.DataAccess.Repositories.Abstract;
using ImoutoRebirth.Room.Database;
using ImoutoRebirth.Room.Database.Entities;
using Microsoft.EntityFrameworkCore;

namespace ImoutoRebirth.Room.DataAccess.Repositories
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

        public async Task<SourceFolder> Update(SourceFolderUpdateData updateData)
        {
            var folder = await _roomDbContext
                .SourceFolders
                .FirstOrDefaultAsync(x => x.Id == updateData.Id);

            if (folder == null)
                throw new EntityNotFoundException<SourceFolderEntity>(updateData.Id);
            
            folder.Path = updateData.Path;
            folder.SupportedExtensionCollection = updateData.SupportedExtensions;
            folder.ShouldAddTagFromFilename = updateData.ShouldAddTagFromFilename;
            folder.ShouldCreateTagsFromSubfolders = updateData.ShouldCreateTagsFromSubfolders;
            folder.ShouldCheckFormat = updateData.ShouldCheckFormat;
            folder.ShouldCheckHashFromName = updateData.ShouldCheckHashFromName;

            await _roomDbContext.SaveChangesAsync();

            return _mapper.Map<SourceFolder>(folder);
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