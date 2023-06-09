﻿using ImoutoRebirth.Common.Cqrs.Events;
using ImoutoRebirth.Meido.Core.ParsingStatus.Events;
using Microsoft.Extensions.Logging;

namespace ImoutoRebirth.Meido.Services.DomainEventHandlers;

public class DisallowedStatusTransferHandler : DomainEventNotificationHandler<DisallowedStatusTransfer>
{
    private readonly ILogger<DisallowedStatusTransferHandler> _logger;

    public DisallowedStatusTransferHandler(ILogger<DisallowedStatusTransferHandler> logger)
    {
        _logger = logger;
    }

    protected override Task Handle(DisallowedStatusTransfer domainEvent, CancellationToken cancellationToken)
    {
        _logger.LogWarning(
            "Parsing status was trying to set to incorrect status {NewStatus} from {Status} for file {FileId} with md5 {Md5} in source {Source} with {FileIdFromSource}",
            domainEvent.NewStatus,
            domainEvent.Model.Status,
            domainEvent.Model.FileId,
            domainEvent.Model.Md5,
            domainEvent.Model.Source,
            domainEvent.Model.FileIdFromSource);
            
        return Task.CompletedTask;
    }
}
