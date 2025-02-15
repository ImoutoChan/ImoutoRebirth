﻿using ImoutoRebirth.Meido.Application.SourceActualizingStateSlice.Commands;
using ImoutoRebirth.Meido.Domain;
using ImoutoRebirth.Meido.MessageContracts;
using MassTransit;
using MediatR;
using Microsoft.Extensions.Logging;

namespace ImoutoRebirth.Meido.UI.Consumers;

public class TagsUpdatedCommandConsumer : IConsumer<TagsUpdatedCommand>
{
    private readonly IMediator _mediator;
    private readonly ILogger<TagsUpdatedCommandConsumer> _logger;

    public TagsUpdatedCommandConsumer(IMediator mediator, ILogger<TagsUpdatedCommandConsumer> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<TagsUpdatedCommand> context)
    {
        if (!context.Message.PostIds.Any())
        {
            _logger.LogWarning("Empty result of notes update.");
            return;
        }

        var command = new MarkTagsUpdatedCommand(
            (MetadataSource)context.Message.SourceId,
            context.Message.PostIds.Distinct().ToList(),
            context.Message.LastHistoryId);

        await _mediator.Send(command);
    }
}
