using ImoutoRebirth.Common.Cqrs.Abstract;
using ImoutoRebirth.Meido.Core.SourceActualizingState;
using MediatR;
using NodaTime.Extensions;

namespace ImoutoRebirth.Meido.Services.MetadataActualizer.CqrsCommands;

internal class NotesUpdatedCommandHandler : ICommandHandler<NotesUpdatedCommand>
{
    private readonly ISourceActualizerService _sourceActualizerService;

    public NotesUpdatedCommandHandler(ISourceActualizerService sourceActualizerService)
    {
        _sourceActualizerService = sourceActualizerService;
    }

    public async Task Handle(NotesUpdatedCommand request, CancellationToken cancellationToken)
    {
        await _sourceActualizerService.MarkNotesUpdated(
            request.SourceId,
            request.PostIds,
            request.LastNoteUpdateDate.ToInstant());
    }
}
