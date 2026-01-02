using ImoutoRebirth.Common.Cqrs.Events;
using ImoutoRebirth.Meido.Application.Infrastructure;
using ImoutoRebirth.Meido.Domain.ParsingStatusAggregate;

namespace ImoutoRebirth.Meido.Application.ParsingStatusSlice;

internal class ParsingStatusUpdatedDomainEventHandler : DomainEventNotificationHandler<ParsingStatusUpdated>
{
    private readonly ISourceMetadataRequester _metadataRequester;

    public ParsingStatusUpdatedDomainEventHandler(ISourceMetadataRequester metadataRequester)
        => _metadataRequester = metadataRequester;

    protected override Task Handle(ParsingStatusUpdated domainEvent, CancellationToken ct)
        => _metadataRequester.Request(
            domainEvent.Entity.Source,
            domainEvent.Entity.FileId,
            domainEvent.Entity.Md5,
            domainEvent.Entity.FileName);
}
