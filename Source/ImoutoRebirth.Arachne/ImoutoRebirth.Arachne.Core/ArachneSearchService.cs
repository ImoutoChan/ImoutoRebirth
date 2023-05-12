using ImoutoRebirth.Arachne.Core.InfrastructureContracts;
using ImoutoRebirth.Arachne.Core.Models;
using ImoutoRebirth.Common;

namespace ImoutoRebirth.Arachne.Core;

/// <summary>
///     Arachne service performs search for meta information in specified source.
/// </summary>
public class ArachneSearchService : IArachneSearchService
{
    private readonly ISearchEngineProvider _searchEngineProvider;

    public ArachneSearchService(ISearchEngineProvider searchEngineProvider)
    {
        _searchEngineProvider = searchEngineProvider;
    }

    public async Task<IReadOnlyCollection<SearchResult>> SearchInAllEngines(Image searchFor) 
        => await _searchEngineProvider
            .GetAll()
            .Select(x => x.Search(searchFor))
            .ToArrayAsync();

    public Task<SearchResult> Search(Image searchFor, SearchEngineType searchEngineType)
        => _searchEngineProvider.Get(searchEngineType).Search(searchFor);

    public Task<LoadedTagsHistory> LoadChangesForTagsSinceHistoryId(
        int historyId, 
        SearchEngineType searchEngineType)
        => _searchEngineProvider.Get(searchEngineType)
            .LoadChangesForTagsSinceHistoryId(historyId);

    public Task<LoadedNotesHistory> LoadChangesForNotesSince(
        DateTimeOffset lastProcessedNoteUpdateAt,
        SearchEngineType searchEngineType)
        => _searchEngineProvider.Get(searchEngineType)
            .LoadChangesForNotesSince(lastProcessedNoteUpdateAt);
}