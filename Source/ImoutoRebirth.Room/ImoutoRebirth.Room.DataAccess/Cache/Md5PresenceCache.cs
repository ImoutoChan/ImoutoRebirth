using Microsoft.Extensions.Caching.Memory;

namespace ImoutoRebirth.Room.DataAccess.Cache;

internal interface IMd5PresenceCache
{
    void Set(string md5, bool isPresent);

    bool? IsPresent(string md5);
    
    void Remove(string md5);
}

internal class Md5PresenceCache : IMd5PresenceCache
{
    private const string FilterCacheKeyPrefix = "Md5PresenceCache_";

    private readonly IMemoryCache _cache;

    public Md5PresenceCache(IMemoryCache cache) => _cache = cache;
    
    public void Set(string md5, bool isPresent) => _cache.Set(GetKey(md5), isPresent);
    
    public bool? IsPresent(string md5) => _cache.Get<bool?>(GetKey(md5));
    
    public void Remove(string md5) => _cache.Remove(GetKey(md5));
    
    private static string GetKey(string md5Hash) => FilterCacheKeyPrefix + md5Hash.ToLowerInvariant();
}
