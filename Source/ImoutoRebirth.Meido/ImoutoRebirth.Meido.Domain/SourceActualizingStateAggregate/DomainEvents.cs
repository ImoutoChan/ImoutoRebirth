using ImoutoRebirth.Common.Domain;

namespace ImoutoRebirth.Meido.Domain.SourceActualizingStateAggregate;

public record ActualizationRequestedDomainEvent(SourceActualizingState Entity) : IDomainEvent;

public record PostsUpdatedDomainEvent(SourceActualizingState Entity, IReadOnlyCollection<string> PostsIdsWithUpdatedNotes)
    : IDomainEvent;
