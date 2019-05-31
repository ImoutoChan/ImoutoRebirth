using System;
using ImoutoRebirth.Common;
using ImoutoRebirth.Common.Domain;

namespace ImoutoRebirth.Meido.Core.SourceActualizingState
{
    public class SourceActualizingState : Entity
    {
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

        public MetadataSource Source { get; }

        public int LastProcessedTagHistoryId { get; private set; }

        public DateTimeOffset LastProcessedTagUpdateAt { get; private set; }

        public DateTimeOffset LastProcessedNoteUpdateAt { get; private set; }

        public DateTimeOffset LastRequested { get; private set; }

        public SourceActualizingState Create(MetadataSource source)
        {
            ArgumentValidator.IsEnumDefined(() => source);

            return new SourceActualizingState(source, 0, DateTimeOffset.Now, DateTimeOffset.Now);
        }

        public void SetLastTagUpdate(int lastProcessedTagUpdateId)
        {
            LastProcessedTagHistoryId = lastProcessedTagUpdateId;
            LastProcessedTagUpdateAt = DateTimeOffset.Now;
        }

        public void SetLastNoteUpdate(DateTimeOffset lastProcessedNoteUpdateAt)
        {
            LastProcessedNoteUpdateAt = lastProcessedNoteUpdateAt;
        }

        public void RequestActualization()
        {
            LastRequested = DateTimeOffset.Now;
            Add(new ActualizationRequested(this));
        }
    }
}