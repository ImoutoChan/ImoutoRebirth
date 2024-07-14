using ImoutoRebirth.Common.Cqrs.Events;
using ImoutoRebirth.Room.Application.Services;
using ImoutoRebirth.Room.Domain.CollectionAggregate;
using MediatR;

namespace ImoutoRebirth.Room.Application.Cqrs.FoldersSlice;

public class SourceFoldersUpdatedDomainEventHandler : DomainEventNotificationHandler<SourceFoldersUpdatedDomainEvent>
{
    private readonly ISourceFolderWatcher _watcher;
    private readonly IMediator _mediator;

    public SourceFoldersUpdatedDomainEventHandler(ISourceFolderWatcher watcher, IMediator mediator)
    {
        _watcher = watcher;
        _mediator = mediator;
    }

    protected override async Task Handle(SourceFoldersUpdatedDomainEvent domainEvent, CancellationToken ct)
    {
        var folders = await _mediator.Send(new OversawSourceFoldersQuery(), ct);
        await _watcher.Refresh(folders, ct);
    }
}
