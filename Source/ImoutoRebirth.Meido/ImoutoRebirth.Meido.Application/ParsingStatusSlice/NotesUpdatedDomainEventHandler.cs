using ImoutoRebirth.Common.Cqrs.Events;
using ImoutoRebirth.Common.Domain;
using ImoutoRebirth.Meido.Application.Infrastructure;
using ImoutoRebirth.Meido.Domain.SourceActualizingStateAggregate;
using NodaTime;

namespace ImoutoRebirth.Meido.Application.ParsingStatusSlice;

internal class NotesUpdatedDomainEventHandler : DomainEventNotificationHandler<PostsUpdatedDomainEvent>
{
    private readonly IClock _clock;
    private readonly IParsingStatusRepository _parsingStatusRepository;
    private readonly IEventStorage _eventStorage;

    public NotesUpdatedDomainEventHandler(
        IClock clock,
        IParsingStatusRepository parsingStatusRepository,
        IEventStorage eventStorage)
    {
        _clock = clock;
        _parsingStatusRepository = parsingStatusRepository;
        _eventStorage = eventStorage;
    }

    protected override async Task Handle(PostsUpdatedDomainEvent domainEvent, CancellationToken ct)
    {
        var (sourceActualizingState, postsIdsWithUpdatedNotes) = domainEvent;

        var now = _clock.GetCurrentInstant();

        var posts = await _parsingStatusRepository.GetBySourcePostIds(
            postsIdsWithUpdatedNotes,
            sourceActualizingState.Source);

        foreach (var parsingStatus in posts)
        {
            var result = parsingStatus.RequestMetadataUpdate(now);
            _eventStorage.AddRange(result.EventsCollection);
        }
    }
}
