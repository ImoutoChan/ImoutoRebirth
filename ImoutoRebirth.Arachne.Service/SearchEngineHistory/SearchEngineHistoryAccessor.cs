using System.Collections.Concurrent;
using System.Threading;
using ImoutoRebirth.Arachne.Core.Models;

namespace ImoutoRebirth.Arachne.Service.SearchEngineHistory
{
    internal abstract class SearchEngineHistoryAccessor
    {
        private readonly ConcurrentDictionary<SearchEngineType, SemaphoreSlim> _accessor =
            new ConcurrentDictionary<SearchEngineType, SemaphoreSlim>();

        public void ReleaseAccess(SearchEngineType searchEngineType)
        {
            var semaphore = _accessor.GetOrAdd(searchEngineType, type => new SemaphoreSlim(1));
            semaphore.Release();
        }

        public bool GainAccess(SearchEngineType searchEngineType)
        {
            var semaphore = _accessor.GetOrAdd(searchEngineType, type => new SemaphoreSlim(1));
            return semaphore.Wait(0);
        }
    }
}