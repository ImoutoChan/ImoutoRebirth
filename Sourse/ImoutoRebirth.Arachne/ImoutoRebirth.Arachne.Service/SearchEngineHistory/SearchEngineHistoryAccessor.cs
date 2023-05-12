using System.Collections.Concurrent;
using ImoutoRebirth.Arachne.Core.Models;

namespace ImoutoRebirth.Arachne.Service.SearchEngineHistory;

internal abstract class SearchEngineHistoryAccessor
{
    private readonly ConcurrentDictionary<SearchEngineType, SemaphoreSlim> _accessor = new();

    public void ReleaseAccess(SearchEngineType searchEngineType)
    {
        var semaphore = _accessor.GetOrAdd(searchEngineType, _ => new SemaphoreSlim(1));
        semaphore.Release();
    }

    public bool GainAccess(SearchEngineType searchEngineType)
    {
        var semaphore = _accessor.GetOrAdd(searchEngineType, _ => new SemaphoreSlim(1));
        return semaphore.Wait(0);
    }
}
