using System;

namespace ImoutoRebirth.Meido.Core
{
    public class SourceActualizingState
    {
        public MetadataSource Source { get; }

        public int LastProcessedTagHistoryId { get; private set; }

        public DateTimeOffset LastProcessedTagUpdateAt { get; private set; }

        public DateTimeOffset LastProcessedNoteUpdateAt { get; private set; }

        private SourceActualizingState(
            MetadataSource source, 
            int lastProcessedTagHistoryId, 
            DateTimeOffset lastProcessedTagUpdateAt, 
            DateTimeOffset lastProcessedNoteUpdateAt)
        {
            Source = source;
            LastProcessedTagHistoryId = lastProcessedTagHistoryId;
            LastProcessedTagUpdateAt = lastProcessedTagUpdateAt;
            LastProcessedNoteUpdateAt = lastProcessedNoteUpdateAt;
        }

        public SourceActualizingState Create(MetadataSource source, int currentTagHistoryId) 
            => new SourceActualizingState(source, currentTagHistoryId, DateTimeOffset.Now, DateTimeOffset.Now);

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