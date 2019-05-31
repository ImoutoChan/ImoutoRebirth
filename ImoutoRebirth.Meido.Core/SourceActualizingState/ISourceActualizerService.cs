using System;
using System.Threading.Tasks;

namespace ImoutoRebirth.Meido.Core.SourceActualizingState
{
    public interface ISourceActualizerService
    {
        Task RequestActualization();

        Task MarkTagsUpdated(int sourceId, int[] postIds, int lastHistoryId);

        Task MarkNotesUpdated(int sourceId, int[] postIds, DateTimeOffset lastNoteUpdateDate);
    }
}