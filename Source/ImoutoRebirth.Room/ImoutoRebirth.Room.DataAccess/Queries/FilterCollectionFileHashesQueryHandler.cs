using ImoutoRebirth.Common.Cqrs.Abstract;
using ImoutoRebirth.Room.Application.Cqrs.CollectionFileSlice;
using ImoutoRebirth.Room.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace ImoutoRebirth.Room.DataAccess.Queries;

internal class FilterCollectionFileHashesQueryHandler 
    : IQueryHandler<FilterCollectionFileHashesQuery, IReadOnlyCollection<string>>
{
    private const string FilterCacheKeyPrefix = "FilterHashesQuery_";

    private readonly RoomDbContext _roomDbContext;
    private readonly IMemoryCache _cache;

    public FilterCollectionFileHashesQueryHandler(RoomDbContext roomDbContext, IMemoryCache cache)
    {
        _roomDbContext = roomDbContext;
        _cache = cache;
    }

    public async Task<IReadOnlyCollection<string>> Handle(FilterCollectionFileHashesQuery request, CancellationToken ct)
    {
        var md5Hashes = request.Md5Hashes;
        
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

    private static string GetKey(string md5Hash) => FilterCacheKeyPrefix + md5Hash.ToLowerInvariant();
}
