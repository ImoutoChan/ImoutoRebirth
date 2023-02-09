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
    }

    public async Task<bool> AnyWithPath(Guid collectionId, string path)
    {
        var key = collectionId + path;
        if (_cache.TryGetValue(key, out bool value) && value)
            return true;

        var result = await CheckInDatabaseWithRemoved(collectionId, path);

        if (result)
            _cache.Set(key, result);

        return result;
    }

    public async Task<IReadOnlyCollection<CollectionFile>> SearchByQuery(CollectionFilesQuery query)
    {
        var files = BuildFilesQuery(query);

        if (query.Skip.HasValue)
            files = files.Skip(query.Skip.Value);

        if (query.Count.HasValue)
            files = files.Take(query.Count.Value);

        var loaded = await files.ToListAsync();

        return _mapper.Map<CollectionFile[]>(loaded);
    }

    public async Task<int> CountByQuery(CollectionFilesQuery query)
    {
        var files = BuildFilesQuery(query);

        return await files.CountAsync();
    }

    public async Task Remove(Guid id)
    {
        var file = await _roomDbContext.CollectionFiles.FindAsync(id);

        if (file == null)
            throw new EntityNotFoundException<CollectionFileEntity>(id);

        file.IsRemoved = true;

        await _roomDbContext.SaveChangesAsync();
    }

    public async Task<string?> GetWithMd5(Guid collectionId, string md5)
    {
        var key = collectionId + md5;
        if (_cache.TryGetValue(key, out string? path))
            return path;

        var file = await _roomDbContext.CollectionFiles.FirstOrDefaultAsync(
            x => x.Md5 == md5 && x.CollectionId == collectionId);

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

    private async Task<bool> CheckInDatabaseWithRemoved(Guid collectionId, string path)
        => await _roomDbContext
            .CollectionFiles
            .IgnoreQueryFilters()
            .Where(x => x.CollectionId == collectionId)
            .Select(x => x.Path)
            .AnyAsync(x => x == path);

    private async Task<List<string>> GetFromDatabaseWithRemoved(Guid collectionId)
        => await _roomDbContext
            .CollectionFiles
            .IgnoreQueryFilters()
            .Where(x => x.CollectionId == collectionId)
            .Select(x => x.Path)
            .ToListAsync();
}
