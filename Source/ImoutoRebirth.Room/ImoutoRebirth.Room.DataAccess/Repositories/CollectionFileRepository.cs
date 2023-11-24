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

    public async Task<bool> AnyWithPath(Guid collectionId, string path, CancellationToken ct)
    {
        var key = collectionId + path;
        if (_memoryCache.TryGetValue(key, out bool value) && value)
            return true;

        var result = await CheckInDatabaseWithRemoved(collectionId, path, ct);

        if (result)
            _memoryCache.Set(key, result);

        return result;
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

    private async Task<bool> CheckInDatabaseWithRemoved(Guid collectionId, string path, CancellationToken ct)
        => await _roomDbContext
            .CollectionFiles
            .IgnoreQueryFilters()
            .Where(x => x.CollectionId == collectionId)
            .Select(x => x.Path)
            .AnyAsync(x => x == path, cancellationToken: ct);
}
