using ImoutoRebirth.Common.Application;
using ImoutoRebirth.Common.Cqrs.Events;
using ImoutoRebirth.Lilin.Domain.FileInfoAggregate;
using ImoutoRebirth.Meido.MessageContracts;

namespace ImoutoRebirth.Lilin.Application.FileInfoSlice;

internal class FileInfoUpdatedPublishingDomainEventHandler : DomainEventNotificationHandler<FileInfoUpdatedDomainEvent>
{
    private readonly IDistributedCommandBus _distributedCommandBus;

    public FileInfoUpdatedPublishingDomainEventHandler(IDistributedCommandBus distributedCommandBus) 
        => _distributedCommandBus = distributedCommandBus;

    protected override async Task Handle(FileInfoUpdatedDomainEvent domainEvent, CancellationToken ct)
    {
        if (domainEvent.MetadataSource == MetadataSource.Manual)
            return;

        var command = new
        {
            FileId = domainEvent.Aggregate.FileId,
            SourceId = (int) domainEvent.MetadataSource
        };

        await _distributedCommandBus.SendAsync<ISavedCommand>(command, ct);
    }
}
