using ImoutoRebirth.Arachne.MessageContracts.Commands;
using ImoutoRebirth.Common.Application;
using ImoutoRebirth.Common.Cqrs.Events;
using ImoutoRebirth.Meido.Domain.SourceActualizingStateAggregate;
using Microsoft.Extensions.Logging;

namespace ImoutoRebirth.Meido.Application.SourceActualizingStateSlice;

internal class ActualizationRequestedDomainEventHandler 
    : DomainEventNotificationHandler<ActualizationRequestedDomainEvent>
{
    private readonly IDistributedCommandBus _distributedCommandBus;
    private readonly ILogger<ActualizationRequestedDomainEventHandler> _logger;

    public ActualizationRequestedDomainEventHandler(
        ILogger<ActualizationRequestedDomainEventHandler> logger,
        IDistributedCommandBus distributedCommandBus)
    {
        _logger = logger;
        _distributedCommandBus = distributedCommandBus;
    }

    protected override async Task Handle(ActualizationRequestedDomainEvent domainEvent, CancellationToken ct)
    {
        var searchEngineType = domainEvent.Entity.Source;

        _logger.LogInformation(
            "Sending request to update tags and notes history from {MetadataSource}",
            searchEngineType);

        await _distributedCommandBus.SendAsync<ILoadTagHistoryCommand>(new
        {
            SearchEngineType = searchEngineType,
            LastProcessedTagHistoryId = domainEvent.Entity.LastProcessedTagHistoryId
        }, ct);

        await _distributedCommandBus.SendAsync<ILoadNoteHistoryCommand>(new
        {
            SearchEngineType = searchEngineType,
            LastProcessedNoteUpdateAt = domainEvent.Entity.LastProcessedNoteUpdateAt.ToDateTimeOffset()
        }, ct);
    }
}
