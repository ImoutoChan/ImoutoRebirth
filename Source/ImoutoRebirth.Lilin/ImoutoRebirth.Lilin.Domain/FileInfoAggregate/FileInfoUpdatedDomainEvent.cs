using ImoutoRebirth.Common.Domain;

namespace ImoutoRebirth.Lilin.Domain.FileInfoAggregate;

public record FileInfoUpdatedDomainEvent(Guid FileId, MetadataSource MetadataSource) : IDomainEvent;
