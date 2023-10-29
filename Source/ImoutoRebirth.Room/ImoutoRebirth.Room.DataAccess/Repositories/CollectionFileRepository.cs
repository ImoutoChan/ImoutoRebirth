using AutoMapper;
using ImoutoRebirth.Room.DataAccess.Cache;
using ImoutoRebirth.Room.DataAccess.Exceptions;
using ImoutoRebirth.Room.DataAccess.Models;
using ImoutoRebirth.Room.DataAccess.Repositories.Abstract;
using ImoutoRebirth.Room.DataAccess.Repositories.Queries;
using ImoutoRebirth.Room.Database;
using ImoutoRebirth.Room.Database.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace ImoutoRebirth.Room.DataAccess.Repositories;

public class CollectionFileRepository : ICollectionFileRepository
{
    private const string FilterCacheKeyPrefix = "FilterHashesQuery_";

    private readonly RoomDbContext _roomDbContext;
    private readonly IMapper _mapper;
    private readonly ICollectionFileCacheService _collectionFileCacheService;
    private readonly IMemoryCache _cache;

    public CollectionFileRepository(
        RoomDbContext roomDbContext,
        IMapper mapper,
        ICollectionFileCacheService collectionFileCacheService,
        IMemoryCache cache)
    {
        _roomDbContext = roomDbContext;
        _mapper = mapper;
        _collectionFileCacheService = collectionFileCacheService;
        _cache = cache;
    }

    public async Task Add(CollectionFile collectionFile)
    {
        var entity = _mapper.Map<CollectionFileEntity>(collectionFile);

        _collectionFileCacheService.AddToFilter(collectionFile.CollectionId, entity.Path);

        await _roomDbContext.CollectionFiles.AddAsync(entity);
        
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

    public async Task<IReadOnlyCollection<CollectionFile>> SearchByQuery(CollectionFilesQuery query, CancellationToken ct)
    {
        var files = BuildFilesQuery(query);

        if (query.Skip.HasValue)
            files = files.Skip(query.Skip.Value);

        if (query.Count.HasValue)
            files = files.Take(query.Count.Value);

        var loaded = await files.ToListAsync(cancellationToken: ct);

        return _mapper.Map<CollectionFile[]>(loaded);
    }

    public async Task<IReadOnlyCollection<string>> FilterHashesQuery(
        IReadOnlyCollection<string> md5Hashes,
        CancellationToken ct)
    {
        if (!md5Hashes.Any())
            return Array.Empty<string>();

        var present = new List<string>();
        var notInCache = new List<string>();

        foreach (var md5Hash in md5Hashes)
        {
            var key = GetKey(md5Hash);
            if (_cache.TryGetValue(key, out bool value))
            {
                if (value) 
                    present.Add(md5Hash);
            }
            else
            {
                notInCache.Add(md5Hash);
            }
        }
        
        if (notInCache.Count == 0)
            return present;
        
        var loaded = await FilterHashesCoreQuery(notInCache, ct);
        
        foreach (var md5Hash in notInCache)
        {
            var key = GetKey(md5Hash);

            if (loaded.Contains(md5Hash))
            {
                _cache.Set(key, true);
                present.Add(md5Hash);
            }
            else
            {
                _cache.Set(key, false);
            }
        }

        return present;
    }
    
    private async Task<IReadOnlyCollection<string>> FilterHashesCoreQuery(
        IReadOnlyCollection<string> md5Hashes,
        CancellationToken ct)
    {
        if (!md5Hashes.Any())
            return Array.Empty<string>();

        return await _roomDbContext.CollectionFiles
            .Where(x => md5Hashes.Contains(x.Md5))
            .OrderBy(x => x.AddedOn)
            .Select(x => x.Md5)
            .ToListAsync(cancellationToken: ct);
    }

    public async Task<IReadOnlyCollection<Guid>> SearchIdsByQuery(CollectionFilesQuery query, CancellationToken ct)
    {
        var files = BuildFilesQuery(query);

        if (query.Skip.HasValue)
            files = files.Skip(query.Skip.Value);

        if (query.Count.HasValue)
            files = files.Take(query.Count.Value);

        return await files.Select(x => x.Id).ToListAsync(cancellationToken: ct);
    }

    public async Task<int> CountByQuery(CollectionFilesQuery query, CancellationToken ct)
    {
        var files = BuildFilesQuery(query);

        return await files.CountAsync(cancellationToken: ct);
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

        var file = await _roomDbContext.CollectionFiles.FirstOrDefaultAsync(
            x => x.Md5 == md5 && x.CollectionId == collectionId, cancellationToken: ct);

        if (file != null)
            _cache.Set(key, file.OriginalPath);

        return file?.OriginalPath;
    }

    private IQueryable<CollectionFileEntity> BuildFilesQuery(CollectionFilesQuery query)
    {
        var files = _roomDbContext.CollectionFiles.AsQueryable();

        if (query.CollectionId.HasValue)
            files = files.Where(x => x.CollectionId == query.CollectionId.Value);

        if (query.CollectionFileIds?.Any() == true)
            files = files.Where(x => query.CollectionFileIds.Contains(x.Id));

        if (query.Path != null)
            files = files.Where(x => query.Path.Equals(x.Path));

        if (query.Md5 != null && query.Md5.Any())
            files = files.Where(x => query.Md5.Contains(x.Md5));

        files = files.OrderBy(x => x.AddedOn);

        return files;
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
