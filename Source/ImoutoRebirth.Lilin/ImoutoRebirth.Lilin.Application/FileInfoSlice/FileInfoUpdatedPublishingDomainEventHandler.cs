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
        if (domainEvent.MetadataSource is MetadataSource.Manual or MetadataSource.Lamia)
            return;

        var command = new SavedCommand(
            domainEvent.Aggregate.FileId,
            (int)domainEvent.MetadataSource);

        await _distributedCommandBus.SendAsync(command, ct);
    }
}
