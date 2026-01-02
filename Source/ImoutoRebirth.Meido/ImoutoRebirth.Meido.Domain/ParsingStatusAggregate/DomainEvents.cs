using ImoutoRebirth.Common.Domain;

namespace ImoutoRebirth.Meido.Domain.ParsingStatusAggregate;

public record UpdateRequested(ParsingStatus Entity) : IDomainEvent;

public record ParsingStatusCreated(ParsingStatus Entity) : IDomainEvent;

public record ParsingStatusUpdated(ParsingStatus Entity) : IDomainEvent;

public record MetadataNotFound(ParsingStatus Entity) : IDomainEvent;

public record DisallowedStatusTransfer(ParsingStatus Entity, Status NewStatus) : IDomainEvent;
