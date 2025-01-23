using ImoutoRebirth.Common.Cqrs.Abstract;
using ImoutoRebirth.Common.Domain;
using ImoutoRebirth.Meido.Application.Infrastructure;
using ImoutoRebirth.Meido.Domain;
using NodaTime;

namespace ImoutoRebirth.Meido.Application.SourceActualizingStateSlice.Commands;

public record MarkNotesUpdatedCommand(
        MetadataSource Source,
        IReadOnlyCollection<string> PostIds,
        Instant LastNoteUpdateDate)
    : ICommand;

internal class MarkNotesUpdatedCommandHandler : ICommandHandler<MarkNotesUpdatedCommand>
{
    private readonly ISourceActualizingStateRepository _sourceActualizingStateRepository;
    private readonly IEventStorage _eventStorage;

    public MarkNotesUpdatedCommandHandler(ISourceActualizingStateRepository sourceActualizingStateRepository, IEventStorage eventStorage)
    {
        _sourceActualizingStateRepository = sourceActualizingStateRepository;
        _eventStorage = eventStorage;
    }

    public async Task Handle(MarkNotesUpdatedCommand request, CancellationToken ct)
    {
        var (source, postIds, lastNoteUpdateDate) = request;
        
        var currentSourceState = await _sourceActualizingStateRepository.Get(source);

        var result = currentSourceState.MarkNotesUpdated(lastNoteUpdateDate, postIds);
        
        _eventStorage.AddRange(result.EventsCollection);
    }
}
