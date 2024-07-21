using ImoutoRebirth.Room.Application.Services;
using ImoutoRebirth.Room.DataAccess.Cache;
using ImoutoRebirth.Room.DataAccess.Exceptions;
using ImoutoRebirth.Room.DataAccess.Mappers;
using ImoutoRebirth.Room.Database;
using ImoutoRebirth.Room.Database.Entities;
using ImoutoRebirth.Room.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace ImoutoRebirth.Room.DataAccess.Repositories;

internal class CollectionFileRepository : ICollectionFileRepository
{
    private readonly RoomDbContext _roomDbContext;
    private readonly ICollectionFileCacheService _collectionFileCacheService;
    private readonly IMd5PresenceCache _md5PresenceCache;
    private readonly IMemoryCache _memoryCache;

    public CollectionFileRepository(
        RoomDbContext roomDbContext,
        ICollectionFileCacheService collectionFileCacheService,
        IMd5PresenceCache md5PresenceCache,
        IMemoryCache memoryCache)
    {
        _roomDbContext = roomDbContext;
        _collectionFileCacheService = collectionFileCacheService;
        _md5PresenceCache = md5PresenceCache;
        _memoryCache = memoryCache;
    }

    public async Task Create(CollectionFile collectionFile)
    {
        var entity = collectionFile.ToEntity();

        _collectionFileCacheService.AddToFilter(collectionFile.CollectionId, entity.Path);

        await _roomDbContext.CollectionFiles.AddAsync(entity);
        await _roomDbContext.SaveChangesAsync();
        
        _md5PresenceCache.Remove(entity.Md5);
    }

    public async Task<IReadOnlyCollection<string>> FilterOutExistingPaths(
        Guid collectionId,
        IReadOnlyCollection<string> inputPaths,
        CancellationToken ct = default)
    {
        var pathExistsStatus = new Dictionary<string, bool?>(inputPaths.Count);

        foreach (var inputPath in inputPaths)
        {
            var cacheKey = collectionId + inputPath;
            pathExistsStatus[inputPath] = _memoryCache.TryGetValue(cacheKey, out bool value) ? value : null;
        }

        var checkInDatabasePaths = pathExistsStatus
            .Where(x => x.Value == null)
            .Select(x => x.Key)
            .ToList();

        if (checkInDatabasePaths.Any())
        {
            var found = await _roomDbContext.CollectionFiles
                .IgnoreQueryFilters()
                .Where(x => x.CollectionId == collectionId)
                .Select(x => x.Path)
                .Where(x => checkInDatabasePaths.Contains(x))
                .ToListAsync(cancellationToken: ct);

            foreach (var checkInDatabasePath in checkInDatabasePaths)
            {
                pathExistsStatus[checkInDatabasePath] = found.Contains(checkInDatabasePath);
                
                var cacheKey = collectionId + checkInDatabasePath;
                _memoryCache.Set(cacheKey, found.Contains(checkInDatabasePath));
            }
        }

        return pathExistsStatus.Where(x => x.Value == false).Select(x => x.Key).ToList();
    }

    public async Task Remove(Guid id)
    {
        var file = await _roomDbContext.CollectionFiles.FindAsync(id);

        if (file == null)
            throw new EntityNotFoundException<CollectionFileEntity>(id);

        file.IsRemoved = true;

        await _roomDbContext.SaveChangesAsync();
        
        _memoryCache.Remove(file.CollectionId + file.Path);
        _memoryCache.Remove(file.CollectionId + file.Md5);
        _md5PresenceCache.Remove(file.Md5);
    }

    public async Task<string?> GetWithMd5(Guid collectionId, string md5, CancellationToken ct)
    {
        var key = collectionId + md5;
        if (_memoryCache.TryGetValue(key, out string? path))
            return path;

        var file = await _roomDbContext.CollectionFiles.AsNoTracking().FirstOrDefaultAsync(
            x => x.Md5 == md5 && x.CollectionId == collectionId, cancellationToken: ct);

        if (file != null)
            _memoryCache.Set(key, file.OriginalPath);

        return file?.OriginalPath;
    }
}
