using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using EFSecondLevelCache.Core;
using ImoutoRebirth.Room.DataAccess.Cache;
using ImoutoRebirth.Room.DataAccess.Models;
using ImoutoRebirth.Room.DataAccess.Repositories.Abstract;
using ImoutoRebirth.Room.Database;
using ImoutoRebirth.Room.Database.Entities;
using Microsoft.EntityFrameworkCore;

namespace ImoutoRebirth.Room.DataAccess.Repositories
{
    public class CollectionFileRepository : ICollectionFileRepository
    {
        private readonly RoomDbContext _roomDbContext;
        private readonly IMapper _mapper;
        private readonly ICollectionFileCacheService _collectionFileCacheService;

        public CollectionFileRepository(
            RoomDbContext roomDbContext,
            IMapper mapper, 
            ICollectionFileCacheService collectionFileCacheService)
        {
            _roomDbContext = roomDbContext;
            _mapper = mapper;
            _collectionFileCacheService = collectionFileCacheService;
        }

        public Task Add(CollectionFile collectionFile)
        {
            var entity = _mapper.Map<CollectionFileEntity>(collectionFile);

            _collectionFileCacheService.AddToFilter(collectionFile.CollectionId, entity.Path);

            return _roomDbContext.CollectionFiles.AddAsync(entity);
        }

        public async Task<bool> HasFile(Guid collectionId, string path)
        {
            return await _collectionFileCacheService.GetResultOrCreateFilterAsync(
                collectionId,
                path,
                CheckInDatabase,
                GetFromDatabase);
        }

        private async Task<bool> CheckInDatabase(Guid collectionId, string path)
            => await _roomDbContext
                       .CollectionFiles
                       .Where(x => x.CollectionId == collectionId)
                       .Select(x => x.Path)
                       .Cacheable()
                       .AnyAsync(x => x == path);

        private async Task<List<string>> GetFromDatabase(Guid collectionId)
            => await _roomDbContext
                    .CollectionFiles.Where(x => x.CollectionId == collectionId)
                    .Select(x => x.Path)
                    .ToListAsync();
    }
}
