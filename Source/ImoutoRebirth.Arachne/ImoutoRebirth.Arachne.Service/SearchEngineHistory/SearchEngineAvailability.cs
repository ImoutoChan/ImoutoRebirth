using System.Collections.Concurrent;
using ImoutoRebirth.Arachne.Core.Models;

namespace ImoutoRebirth.Arachne.Service.SearchEngineHistory;

public class SearchEngineAvailability
{
    private readonly ConcurrentDictionary<SearchEngineType, bool> _availability = new();

    public bool IsAvailable(SearchEngineType source) => _availability.TryGetValue(source, out var available) && available;

    public void Set(SearchEngineType source, bool available) => _availability[source] = available;
}
