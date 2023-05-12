using System.Collections.Concurrent;

namespace ImoutoRebirth.Room.DataAccess.Cache;

/// <summary>
/// Ensure singleton, service uses Bloom Filter, guarantees only false responses
/// </summary>
public class CollectionFileCacheService : ICollectionFileCacheService
{
    // Guid - collection id
    private readonly ConcurrentDictionary<Guid, BloomFilter<string>> _bloomFilters
        = new ConcurrentDictionary<Guid, BloomFilter<string>>();

    private readonly SemaphoreSlim _semaphoreSlim = new SemaphoreSlim(1);

    public async Task<bool> GetResultOrCreateFilterAsync(
        Guid id, 
        string value, 
        Func<Guid, string, Task<bool>> checkFunc, 
        Func<Guid, Task<List<string>>> addFunc)
    {
        if (_bloomFilters.TryGetValue(id, out var filter))
            return filter.Contains(value) && await checkFunc(id, value);


        await _semaphoreSlim.WaitAsync();
        try
        {
            if (_bloomFilters.TryGetValue(id, out filter))
                return filter.Contains(value) && await checkFunc(id, value);

            var paths = await addFunc(id);

            filter = new BloomFilter<string>(200000);

            Parallel.ForEach(paths, s => filter.Add(s));

            _bloomFilters.TryAdd(id, filter);

            return paths.Any(x => x == value);
        }
        finally
        {
            _semaphoreSlim.Release();
        }
    }

    public void AddToFilter(Guid id, string value)
    {
        if (_bloomFilters.TryGetValue(id, out var filter))
            filter.Add(value);
    }
}