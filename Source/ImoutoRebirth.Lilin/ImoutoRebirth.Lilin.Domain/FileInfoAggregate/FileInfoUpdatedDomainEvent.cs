using ImoutoRebirth.Common.Domain;

namespace ImoutoRebirth.Lilin.Domain.FileInfoAggregate;

public record FileInfoUpdatedDomainEvent(FileInfo Aggregate, MetadataSource MetadataSource) : IDomainEvent;
