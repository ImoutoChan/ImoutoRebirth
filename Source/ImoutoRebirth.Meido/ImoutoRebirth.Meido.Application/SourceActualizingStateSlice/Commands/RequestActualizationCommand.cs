using ImoutoRebirth.Common.Cqrs.Abstract;
using ImoutoRebirth.Common.Domain;
using ImoutoRebirth.Meido.Application.Infrastructure;
using ImoutoRebirth.Meido.Domain;
using NodaTime;

namespace ImoutoRebirth.Meido.Application.SourceActualizingStateSlice.Commands;

public record RequestActualizationCommand(IReadOnlyCollection<MetadataSource> ActiveSources) : ICommand;

internal class RequestActualizationCommandHandler : ICommandHandler<RequestActualizationCommand>
{
    private readonly IEventStorage _eventStorage;
    private readonly ISourceActualizingStateRepository _sourceActualizingStateRepository;
    private readonly IClock _clock;

    public RequestActualizationCommandHandler(
        ISourceActualizingStateRepository sourceActualizingStateRepository,
        IEventStorage eventStorage,
        IClock clock)
    {
        _sourceActualizingStateRepository = sourceActualizingStateRepository;
        _eventStorage = eventStorage;
        _clock = clock;
    }

    public async Task Handle(RequestActualizationCommand request, CancellationToken ct)
    {
        var now = _clock.GetCurrentInstant();
        var states = await _sourceActualizingStateRepository.GetAll();
        var activeStates = states.Where(x => request.ActiveSources.Contains(x.Source));

        foreach (var state in activeStates)
        {
            var result = state.RequestActualization(now);
            _eventStorage.AddRange(result.EventsCollection);
        }
    }
}
