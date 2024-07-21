using ImoutoRebirth.Common.Cqrs.Abstract;
using ImoutoRebirth.Room.Application.Cqrs.CollectionFileSlice;
using ImoutoRebirth.Room.DataAccess.Cache;
using ImoutoRebirth.Room.Database;
using Microsoft.EntityFrameworkCore;

namespace ImoutoRebirth.Room.DataAccess.Queries;

internal class FilterCollectionFileHashesQueryHandler 
    : IQueryHandler<FilterCollectionFileHashesQuery, IReadOnlyCollection<string>>
{
    private readonly RoomDbContext _roomDbContext;
    private readonly IMd5PresenceCache _cache;

    public FilterCollectionFileHashesQueryHandler(RoomDbContext roomDbContext, IMd5PresenceCache cache)
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
            var value = _cache.IsPresent(md5Hash);
            if (value != null)
            {
                if (value.Value) 
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
            if (loaded.Contains(md5Hash))
            {
                _cache.Set(md5Hash, true);
                present.Add(md5Hash);
            }
            else
            {
                _cache.Set(md5Hash, false);
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
}
