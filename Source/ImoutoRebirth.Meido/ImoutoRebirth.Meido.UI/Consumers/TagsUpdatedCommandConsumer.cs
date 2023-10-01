using ImoutoRebirth.Meido.Application.SourceActualizingStateSlice.Commands;
using ImoutoRebirth.Meido.Domain;
using ImoutoRebirth.Meido.MessageContracts;
using MassTransit;
using MassTransit.Mediator;
using Microsoft.Extensions.Logging;

namespace ImoutoRebirth.Meido.UI.Consumers;

internal class TagsUpdatedCommandConsumer : IConsumer<ITagsUpdatedCommand>
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

        var command = new MarkTagsUpdatedCommand(
            (MetadataSource)context.Message.SourceId,
            context.Message.PostIds,
            context.Message.LastHistoryId);

        await _mediator.Send(command);
    }
}
