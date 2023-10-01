using ImoutoRebirth.Common.Cqrs.Abstract;
using ImoutoRebirth.Common.Domain;
using ImoutoRebirth.Meido.Application.Infrastructure;
using ImoutoRebirth.Meido.Domain;
using NodaTime;

namespace ImoutoRebirth.Meido.Application.SourceActualizingStateSlice.Commands;

public record MarkTagsUpdatedCommand(
        MetadataSource Source,
        IReadOnlyCollection<int> PostIds,
        int LastHistoryId)
    : ICommand;

internal class MarkTagsUpdatedCommandHandler : ICommandHandler<MarkTagsUpdatedCommand>
{
    private readonly IEventStorage _eventStorage;
    private readonly ISourceActualizingStateRepository _sourceActualizingStateRepository;
    private readonly IClock _clock;

    public MarkTagsUpdatedCommandHandler(
        ISourceActualizingStateRepository sourceActualizingStateRepository,
        IEventStorage eventStorage,
        IClock clock)
    {
        _sourceActualizingStateRepository = sourceActualizingStateRepository;
        _eventStorage = eventStorage;
        _clock = clock;
    }

    public async Task Handle(MarkTagsUpdatedCommand request, CancellationToken ct)
    {
        var (source, postIds, lastHistoryId) = request;
        var now = _clock.GetCurrentInstant();

        var currentSourceState = await _sourceActualizingStateRepository.Get(source);

        var result = currentSourceState.MarkTagsUpdated(lastHistoryId, postIds, now);

        _eventStorage.AddRange(result.EventsCollection);
    }
}
