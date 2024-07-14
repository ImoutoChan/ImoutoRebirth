using ImoutoRebirth.Common.Cqrs.Abstract;
using ImoutoRebirth.Room.Application.Services;

namespace ImoutoRebirth.Room.Application.Cqrs;

public record OversawSourceFoldersQuery : IQuery<IReadOnlyCollection<string>>;

internal class OversawSourceFoldersQueryHandler : IQueryHandler<OversawSourceFoldersQuery, IReadOnlyCollection<string>>
{
    private readonly ICollectionRepository _collectionRepository;

    public OversawSourceFoldersQueryHandler(ICollectionRepository collectionRepository) 
        => _collectionRepository = collectionRepository;

    public async Task<IReadOnlyCollection<string>> Handle(OversawSourceFoldersQuery _, CancellationToken ct)
    {
        var collections = await _collectionRepository.GetAll(ct);
        return collections.SelectMany(x => x.SourceFolders.Select(y => y.Path)).Distinct().ToList();
    }
}
