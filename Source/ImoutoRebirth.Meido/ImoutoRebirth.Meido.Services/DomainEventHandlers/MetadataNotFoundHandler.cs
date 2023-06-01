using ImoutoRebirth.Common.Cqrs.Events;
using ImoutoRebirth.Meido.Core.ParsingStatus.Events;

namespace ImoutoRebirth.Meido.Services.DomainEventHandlers;

public class MetadataNotFoundHandler : DomainEventNotificationHandler<MetadataNotFound>
{
    protected override Task Handle(MetadataNotFound domainEvent, CancellationToken cancellationToken)
    {
        // _logger.LogInformation("Sending request to search original");
        // todo: send search original request
        return Task.CompletedTask;
    }
}
