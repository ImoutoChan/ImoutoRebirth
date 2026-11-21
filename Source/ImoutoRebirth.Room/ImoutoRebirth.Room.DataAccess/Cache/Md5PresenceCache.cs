using Microsoft.Extensions.Caching.Memory;

namespace ImoutoRebirth.Room.DataAccess.Cache;

internal interface IMd5PresenceCache
{
    Task<IReadOnlyCollection<string>> FindPresent(
        IReadOnlyCollection<string> hashes,
        Func<IReadOnlyCollection<string>, CancellationToken, Task<IReadOnlyCollection<string>>> loadMissing,
        CancellationToken ct);

    void Remove(string md5);

    void Clear();
}

internal class Md5PresenceCache : IMd5PresenceCache, IDisposable
{
    private readonly MemoryCache _cache = new(new MemoryCacheOptions());
    private readonly Lock _sync = new();
    private long _revision;

    public async Task<IReadOnlyCollection<string>> FindPresent(
        IReadOnlyCollection<string> hashes,
        Func<IReadOnlyCollection<string>, CancellationToken, Task<IReadOnlyCollection<string>>> loadMissing,
        CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();

        var normalized = hashes.Select(hash => hash.ToLowerInvariant()).Distinct();
        var present = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var missing = new List<string>();
        long revision;

        lock (_sync)
        {
            revision = _revision;

            foreach (var hash in normalized)
            {
                if (!_cache.TryGetValue<bool>(hash, out var isPresent))
                {
                    missing.Add(hash);
                }
                else if (isPresent)
                {
                    present.Add(hash);
                }
            }
        }

        if (missing.Count != 0)
        {
            present.UnionWith(await loadMissing(missing, ct));
            ct.ThrowIfCancellationRequested();

            RememberIfUnchanged(missing, present, revision);
        }

        return hashes.Where(present.Contains).ToArray();
    }

    public void Remove(string md5)
    {
        lock (_sync)
        {
            _revision++;
            _cache.Remove(md5.ToLowerInvariant());
        }
    }

    public void Clear()
    {
        lock (_sync)
        {
            _revision++;
            _cache.Clear();
        }
    }

    public void Dispose() => _cache.Dispose();

    private void RememberIfUnchanged(IReadOnlyCollection<string> missing, IReadOnlySet<string> present, long revision)
    {
        lock (_sync)
        {
            if (_revision != revision)
                return;

            foreach (var hash in missing)
                _cache.Set(hash, present.Contains(hash));
        }
    }
}
