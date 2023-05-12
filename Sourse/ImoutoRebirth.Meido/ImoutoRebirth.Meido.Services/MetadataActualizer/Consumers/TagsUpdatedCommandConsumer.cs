using ImoutoRebirth.Meido.MessageContracts;
using ImoutoRebirth.Meido.Services.MetadataActualizer.CqrsCommands;
using MassTransit;
using MediatR;
using Microsoft.Extensions.Logging;

namespace ImoutoRebirth.Meido.Services.MetadataActualizer.Consumers;

public class TagsUpdatedCommandConsumer : IConsumer<ITagsUpdatedCommand>
{
    private readonly IMediator _mediator;
    private readonly ILogger<TagsUpdatedCommandConsumer> _logger;

    public TagsUpdatedCommandConsumer(IMediator mediator, ILogger<TagsUpdatedCommandConsumer> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<ITagsUpdatedCommand> context)
    {
        if (!context.Message.PostIds.Any())
        {
            _logger.LogWarning("Empty result of notes update.");
            return;
        }

        var command = new TagsUpdatedCommand(
            context.Message.SourceId,
            context.Message.PostIds,
            context.Message.LastHistoryId);

        await _mediator.Send(command);
    }
}