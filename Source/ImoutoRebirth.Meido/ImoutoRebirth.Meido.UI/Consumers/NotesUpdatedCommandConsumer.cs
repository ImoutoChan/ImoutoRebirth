using ImoutoRebirth.Meido.Application.SourceActualizingStateSlice.Commands;
using ImoutoRebirth.Meido.Domain;
using ImoutoRebirth.Meido.MessageContracts;
using MassTransit;
using MediatR;
using Microsoft.Extensions.Logging;
using NodaTime.Extensions;

namespace ImoutoRebirth.Meido.UI.Consumers;

internal class NotesUpdatedCommandConsumer : IConsumer<INotesUpdatedCommand>
{
    private readonly IMediator _mediator;
    private readonly ILogger<NotesUpdatedCommandConsumer> _logger;

    public NotesUpdatedCommandConsumer(IMediator mediator, ILogger<NotesUpdatedCommandConsumer> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<INotesUpdatedCommand> context)
    {
        if (!context.Message.PostIds.Any())
        {
            _logger.LogWarning("Empty result of notes update.");
            return;
        }

        var command = new MarkNotesUpdatedCommand(
            (MetadataSource)context.Message.SourceId,
            context.Message.PostIds,
            context.Message.LastNoteUpdateDate.ToInstant());

        await _mediator.Send(command);
    }
}
