using ImoutoRebirth.Common;
using ImoutoRebirth.Common.Domain;
using NodaTime;

namespace ImoutoRebirth.Meido.Domain.SourceActualizingStateAggregate;

public class SourceActualizingState
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

    public DomainResult RequestActualization(Instant now)
    {
        LastRequested = now;
        return new DomainResult(new ActualizationRequestedDomainEvent(this));
    }

    public DomainResult MarkNotesUpdated(
        Instant lastProcessedNoteUpdateAt,
        IReadOnlyCollection<int> postsWithUpdatedNotesIds)
    {
        LastProcessedNoteUpdateAt = lastProcessedNoteUpdateAt;

        return new DomainResult(new PostsUpdatedDomainEvent(this, postsWithUpdatedNotesIds));
    }

    public DomainResult MarkTagsUpdated(
        int lastProcessedTagUpdateId,
        IReadOnlyCollection<int> postsWithUpdatedNotesIds, 
        Instant now)
    {
        LastProcessedTagHistoryId = lastProcessedTagUpdateId;
        LastProcessedTagUpdateAt = now;

        return new DomainResult(new PostsUpdatedDomainEvent(this, postsWithUpdatedNotesIds));
    }
}


