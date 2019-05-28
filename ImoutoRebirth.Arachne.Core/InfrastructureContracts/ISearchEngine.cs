using System;
using System.Threading.Tasks;
using ImoutoRebirth.Arachne.Core.Models;

namespace ImoutoRebirth.Arachne.Core.InfrastructureContracts
{
    public interface ISearchEngine
    {
        SearchEngineType SearchEngineType { get; }

        Task<SearchResult> Search(Image image);

        Task<LoadedTagsHistory> LoadChangesForTagsSinceHistoryId(int historyId);

        Task<LoadedNotesHistory> LoadChangesForNotesSince(DateTimeOffset lastProcessedNoteUpdateAt);
    }
}