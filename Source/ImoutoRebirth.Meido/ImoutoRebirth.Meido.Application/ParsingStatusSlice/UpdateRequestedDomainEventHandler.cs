using ImoutoRebirth.Common.Cqrs.Events;
using ImoutoRebirth.Meido.Application.Infrastructure;
using ImoutoRebirth.Meido.Domain.ParsingStatusAggregate;

namespace ImoutoRebirth.Meido.Application.ParsingStatusSlice;

internal class UpdateRequestedDomainEventHandler : DomainEventNotificationHandler<UpdateRequested>
{
    private readonly ISourceMetadataRequester _metadataRequester;

    public UpdateRequestedDomainEventHandler(ISourceMetadataRequester metadataRequester) 
        => _metadataRequester = metadataRequester;

    protected override async Task Handle(UpdateRequested domainEvent, CancellationToken cancellationToken) 
        => await _metadataRequester.Request(domainEvent.Entity.Source, domainEvent.Entity.FileId, domainEvent.Entity.Md5);
}
