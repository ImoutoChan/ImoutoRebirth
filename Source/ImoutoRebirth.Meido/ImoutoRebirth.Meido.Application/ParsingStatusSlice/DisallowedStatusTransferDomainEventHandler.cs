using ImoutoRebirth.Common.Cqrs.Events;
using ImoutoRebirth.Meido.Domain.ParsingStatusAggregate;
using Microsoft.Extensions.Logging;

namespace ImoutoRebirth.Meido.Application.ParsingStatusSlice;

internal class DisallowedStatusTransferDomainEventHandler : DomainEventNotificationHandler<DisallowedStatusTransfer>
{
    private readonly ILogger<DisallowedStatusTransferDomainEventHandler> _logger;

    public DisallowedStatusTransferDomainEventHandler(ILogger<DisallowedStatusTransferDomainEventHandler> logger) 
        => _logger = logger;

    protected override Task Handle(DisallowedStatusTransfer domainEvent, CancellationToken ct)
    {
        _logger.LogWarning(
            "Parsing status was trying to set to incorrect status {NewStatus} from {Status} for file {FileId} with md5 {Md5} in source {Source} with {FileIdFromSource}",
            domainEvent.NewStatus,
            domainEvent.Entity.Status,
            domainEvent.Entity.FileId,
            domainEvent.Entity.Md5,
            domainEvent.Entity.Source,
            domainEvent.Entity.FileIdFromSource);

        return Task.CompletedTask;
    }
}
