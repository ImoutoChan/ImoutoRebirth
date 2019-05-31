using System.Collections.Concurrent;
using System.Threading;
using ImoutoRebirth.Arachne.Core.Models;

namespace ImoutoRebirth.Arachne.Service.SearchEngineHistory
{
    public abstract class SearchEngineHistoryAccessor
    {
        private static readonly ConcurrentDictionary<SearchEngineType, SemaphoreSlim> Accessor =
            new ConcurrentDictionary<SearchEngineType, SemaphoreSlim>();

        public void ReleaseAccess(SearchEngineType searchEngineType)
        {
            var semaphore = Accessor.GetOrAdd(searchEngineType, type => new SemaphoreSlim(1));
            semaphore.Release();
        }

        public bool GainAccess(SearchEngineType searchEngineType)
        {
            var semaphore = Accessor.GetOrAdd(searchEngineType, type => new SemaphoreSlim(1));
            return semaphore.Wait(0);
        }
    }
}