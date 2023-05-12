using ImoutoRebirth.Meido.MessageContracts;
using ImoutoRebirth.Meido.Services.MetadataActualizer.CqrsCommands;
using MassTransit;
using MediatR;
using Microsoft.Extensions.Logging;

namespace ImoutoRebirth.Meido.Services.MetadataActualizer.Consumers;

public class NotesUpdatedCommandConsumer : IConsumer<INotesUpdatedCommand>
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

        var command = new NotesUpdatedCommand(
            context.Message.SourceId,
            context.Message.PostIds,
            context.Message.LastNoteUpdateDate);

        await _mediator.Send(command);
    }
}