using ImoutoRebirth.Common.Cqrs.Events;
using ImoutoRebirth.Meido.Application.Infrastructure;
using ImoutoRebirth.Meido.Domain.ParsingStatusAggregate;

namespace ImoutoRebirth.Meido.Application.ParsingStatusSlice;

internal class ParsingStatusCreatedDomainEventHandler : DomainEventNotificationHandler<ParsingStatusCreated>
{
    private readonly ISourceMetadataRequester _metadataRequester;

    public ParsingStatusCreatedDomainEventHandler(ISourceMetadataRequester metadataRequester) 
        => _metadataRequester = metadataRequester;

    protected override Task Handle(ParsingStatusCreated domainEvent, CancellationToken cancellationToken) 
        => _metadataRequester.Request(domainEvent.Entity.Source, domainEvent.Entity.FileId, domainEvent.Entity.Md5);
}
