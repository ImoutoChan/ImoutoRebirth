using ImoutoRebirth.Common.Cqrs.Abstract;
using ImoutoRebirth.Room.Application.Services;
using ImoutoRebirth.Room.Domain.CollectionAggregate;

namespace ImoutoRebirth.Room.Application.Cqrs.CollectionSlice;

public record AllCollectionsQuery() : IQuery<IReadOnlyCollection<Collection>>;

internal class AllCollectionsQueryHandler : IQueryHandler<AllCollectionsQuery, IReadOnlyCollection<Collection>>
{
    private readonly ICollectionRepository _collectionRepository;

    public AllCollectionsQueryHandler(ICollectionRepository collectionRepository) 
        => _collectionRepository = collectionRepository;

    public async Task<IReadOnlyCollection<Collection>> Handle(AllCollectionsQuery request, CancellationToken ct) 
        => await _collectionRepository.GetAll(ct);
}
