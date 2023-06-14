using ImoutoRebirth.Common.Domain;

namespace ImoutoRebirth.Lilin.Core.FileInfoAggregate;

public record FileInfoUpdatedDomainEvent(Guid FileId, MetadataSource MetadataSource) : IDomainEvent;
