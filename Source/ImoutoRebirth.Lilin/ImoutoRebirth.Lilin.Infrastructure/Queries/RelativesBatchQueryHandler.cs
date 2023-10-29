using ImoutoRebirth.Common.Cqrs.Abstract;
using ImoutoRebirth.Lilin.Application.FileInfoSlice.Queries;
using ImoutoRebirth.Lilin.DataAccess;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using NinjaNye.SearchExtensions;

namespace ImoutoRebirth.Lilin.Infrastructure.Queries;

internal class RelativesBatchQueryHandler : IQueryHandler<RelativesBatchQuery, IReadOnlyCollection<RelativeShortInfo>>
{
    private const string FilterCacheKeyPrefix = "FilterHashes_";
    
    private readonly LilinDbContext _lilinDbContext;
    private readonly IMemoryCache _cache;

    public RelativesBatchQueryHandler(LilinDbContext lilinDbContext, IMemoryCache cache)
    {
        _lilinDbContext = lilinDbContext;
        _cache = cache;
    }

    public async Task<IReadOnlyCollection<RelativeShortInfo>> Handle(
        RelativesBatchQuery query,
        CancellationToken ct)
    {
        var md5Hashes = query.Md5;
        
        if (!md5Hashes.Any())
            return Array.Empty<RelativeShortInfo>();

        var inCache = new List<RelativeShortInfo>(md5Hashes.Count);
        var notInCache = new List<string>(md5Hashes.Count);

        foreach (var md5Hash in md5Hashes)
        {
            var key = GetKey(md5Hash);

            if (_cache.TryGetValue<RelativeShortInfo>(key, out var value))
            {
                inCache.Add(value!);
            }
            else
            {
                notInCache.Add(md5Hash);
            }
        }
        
        if (notInCache.Count == 0)
            return inCache;
        
        var loaded = await LoadFromDatabase(notInCache, ct);
        
        foreach (var info in loaded)
        {
            var key = GetKey(info.Hash);
            _cache.Set(key, info);
            inCache.Add(info);
        }

        return inCache;
    }

    private static string GetKey(string md5Hash) => FilterCacheKeyPrefix + md5Hash.ToLowerInvariant();
    
    private async Task<IReadOnlyCollection<RelativeShortInfo>> LoadFromDatabase(
        IReadOnlyCollection<string> hashes,
        CancellationToken ct)
    {
        var request = _lilinDbContext.FileTags
            .Where(x => x.Tag!.Name == "ParentMd5" || x.Tag.Name == "Child")
            .Search(x => x.Value).Containing(hashes.ToArray())
            .Select(x => new
            {
                Value = x.Value,
                TagName = x.Tag!.Name
            });

        var fileTags = await request.ToListAsync(cancellationToken: ct);

        return hashes
            .Select(x =>
            {
                var tags = fileTags.Where(y => y.Value?.Contains(x) == true);

                if (tags.Any(y => y.TagName == "ParentMd5"))
                    return (x, RelativeType.Parent);

                if (tags.Any(y => y.TagName == "Child"))
                    return (x, RelativeType.Child);

                return (x, (RelativeType?)null);
            })
            .Select(x => new RelativeShortInfo(x.x, x.Item2))
            .ToList();
    }
}
