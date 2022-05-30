using System;
using ImoutoRebirth.Common;
using ImoutoRebirth.Common.Domain;
using NodaTime;

namespace ImoutoRebirth.Meido.Core.SourceActualizingState
{
    public class SourceActualizingState : Entity
    {
        private SourceActualizingState(
            MetadataSource source,
            int lastProcessedTagHistoryId,
            Instant lastProcessedTagUpdateAt,
            Instant lastProcessedNoteUpdateAt)
        {
            Source = source;
            LastProcessedTagHistoryId = lastProcessedTagHistoryId;
            LastProcessedTagUpdateAt = lastProcessedTagUpdateAt;
            LastProcessedNoteUpdateAt = lastProcessedNoteUpdateAt;
        }

        public MetadataSource Source { get; }

        public int LastProcessedTagHistoryId { get; private set; }

        public Instant LastProcessedTagUpdateAt { get; private set; }

        public Instant LastProcessedNoteUpdateAt { get; private set; }

        public Instant LastRequested { get; private set; }

        public SourceActualizingState Create(MetadataSource source, Instant now)
        {
            ArgumentValidator.IsEnumDefined(() => source);

            return new SourceActualizingState(source, 0, now, now);
        }

        public void SetLastTagUpdate(int lastProcessedTagUpdateId, Instant now)
        {
            LastProcessedTagHistoryId = lastProcessedTagUpdateId;
            LastProcessedTagUpdateAt = now;
        }

        public void SetLastNoteUpdate(Instant lastProcessedNoteUpdateAt)
        {
            LastProcessedNoteUpdateAt = lastProcessedNoteUpdateAt;
        }

        public void RequestActualization(Instant now)
        {
            LastRequested = now;
            Add(new ActualizationRequested(this));
        }
    }
}
