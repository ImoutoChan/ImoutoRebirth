using NodaTime;

namespace ImoutoRebirth.Meido.Core.SourceActualizingState
{
    public interface ISourceActualizerService
    {
        Task RequestActualization(MetadataSource[] activeSources);

        Task MarkTagsUpdated(int sourceId, int[] postIds, int lastHistoryId);

        Task MarkNotesUpdated(int sourceId, int[] postIds, Instant lastNoteUpdateDate);
    }
}
