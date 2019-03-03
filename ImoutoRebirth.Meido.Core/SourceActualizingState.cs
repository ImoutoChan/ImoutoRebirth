using System;

namespace ImoutoRebirth.Meido.Core
{
    public class SourceActualizingState
    {
        public int SourceId { get; }

        public int LastProcessedTagHistoryId { get; private set; }

        public DateTimeOffset LastProcessedTagUpdateAt { get; private set; }

        public DateTimeOffset LastProcessedNoteUpdateAt { get; private set; }

        private SourceActualizingState(
            int sourceId, 
            int lastProcessedTagHistoryId, 
            DateTimeOffset lastProcessedTagUpdateAt, 
            DateTimeOffset lastProcessedNoteUpdateAt)
        {
            SourceId = sourceId;
            LastProcessedTagHistoryId = lastProcessedTagHistoryId;
            LastProcessedTagUpdateAt = lastProcessedTagUpdateAt;
            LastProcessedNoteUpdateAt = lastProcessedNoteUpdateAt;
        }

        public SourceActualizingState Create(int sourceId, int currentTagHistoryId) 
            => new SourceActualizingState(sourceId, currentTagHistoryId, DateTimeOffset.Now, DateTimeOffset.Now);

        public void SetLastTagUpdate(int lastProcessedTagUpdateId)
        {
            LastProcessedTagHistoryId = lastProcessedTagUpdateId;
            LastProcessedTagUpdateAt = DateTimeOffset.Now;
        }

        public void SetLastNoteUpdate(DateTimeOffset lastProcessedNoteUpdateAt)
        {
            LastProcessedNoteUpdateAt = lastProcessedNoteUpdateAt;
        }
    }
}