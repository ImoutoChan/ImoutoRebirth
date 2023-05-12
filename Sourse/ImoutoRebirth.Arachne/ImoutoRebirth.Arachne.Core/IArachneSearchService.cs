using ImoutoRebirth.Arachne.Core.Models;

namespace ImoutoRebirth.Arachne.Core;

public interface IArachneSearchService
{
    Task<SearchResult> Search(Image searchFor, SearchEngineType searchEngineType);

    Task<IReadOnlyCollection<SearchResult>> SearchInAllEngines(Image searchFor);

    Task<LoadedTagsHistory> LoadChangesForTagsSinceHistoryId(
        int historyId, 
        SearchEngineType searchEngineType);

    Task<LoadedNotesHistory> LoadChangesForNotesSince(
        DateTimeOffset lastProcessedNoteUpdateAt, 
        SearchEngineType searchEngineType);
}