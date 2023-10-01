using ImoutoRebirth.Common.Cqrs.Events;
using ImoutoRebirth.Meido.Application.ParsingStatusSlice.Commands;
using ImoutoRebirth.Meido.Domain.SourceActualizingStateAggregate;
using MediatR;

namespace ImoutoRebirth.Meido.Application.ParsingStatusSlice;

internal class PostsUpdatedDomainEventHandler : DomainEventNotificationHandler<PostsUpdatedDomainEvent>
{
    private readonly IMediator _mediator;

    public PostsUpdatedDomainEventHandler(IMediator mediator) => _mediator = mediator;

    protected override async Task Handle(PostsUpdatedDomainEvent domainEvent, CancellationToken ct)
    {
        var (sourceActualizingState, postsIdsWithUpdatedNotes) = domainEvent;

        await _mediator.Send(
            new RequestMetadataUpdateCommand(postsIdsWithUpdatedNotes, sourceActualizingState.Source),
            ct);
    }
}
