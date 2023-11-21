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
    private const string FilterCacheKeyPrefix = "FilterHashesQuery_";

    private readonly RoomDbContext _roomDbContext;
    private readonly ICollectionFileCacheService _collectionFileCacheService;
    private readonly IMemoryCache _cache;

    public CollectionFileRepository(
        RoomDbContext roomDbContext,
        ICollectionFileCacheService collectionFileCacheService,
        IMemoryCache cache)
    {
        _roomDbContext = roomDbContext;
        _collectionFileCacheService = collectionFileCacheService;
        _cache = cache;
    }

    public async Task Create(CollectionFile collectionFile)
    {
        var entity = collectionFile.ToEntity();

        _collectionFileCacheService.AddToFilter(collectionFile.CollectionId, entity.Path);

        await _roomDbContext.CollectionFiles.AddAsync(entity);
        await _roomDbContext.SaveChangesAsync();
        
        _cache.Remove(GetKey(entity.Md5));
    }

    public async Task<bool> AnyWithPath(Guid collectionId, string path, CancellationToken ct)
    {
        var key = collectionId + path;
        if (_cache.TryGetValue(key, out bool value) && value)
            return true;

        var result = await CheckInDatabaseWithRemoved(collectionId, path, ct);

        if (result)
            _cache.Set(key, result);

        return result;
    }

    public async Task Remove(Guid id)
    {
        var file = await _roomDbContext.CollectionFiles.FindAsync(id);

        if (file == null)
            throw new EntityNotFoundException<CollectionFileEntity>(id);

        file.IsRemoved = true;

        await _roomDbContext.SaveChangesAsync();
        
        _cache.Remove(GetKey(file.Md5));
    }

    public async Task<string?> GetWithMd5(Guid collectionId, string md5, CancellationToken ct)
    {
        var key = collectionId + md5;
        if (_cache.TryGetValue(key, out string? path))
            return path;

        var file = await _roomDbContext.CollectionFiles.AsNoTracking().FirstOrDefaultAsync(
            x => x.Md5 == md5 && x.CollectionId == collectionId, cancellationToken: ct);

        if (file != null)
            _cache.Set(key, file.OriginalPath);

        return file?.OriginalPath;
    }

    private async Task<bool> CheckInDatabaseWithRemoved(Guid collectionId, string path, CancellationToken ct)
        => await _roomDbContext
            .CollectionFiles
            .IgnoreQueryFilters()
            .Where(x => x.CollectionId == collectionId)
            .Select(x => x.Path)
            .AnyAsync(x => x == path, cancellationToken: ct);
    
    private static string GetKey(string md5Hash) => FilterCacheKeyPrefix + md5Hash.ToLowerInvariant();
}
