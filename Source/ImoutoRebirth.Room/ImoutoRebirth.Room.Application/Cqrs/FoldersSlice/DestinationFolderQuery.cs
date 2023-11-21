using ImoutoRebirth.Common.Cqrs.Abstract;
using ImoutoRebirth.Common.Domain;
using ImoutoRebirth.Room.Application.Services;

namespace ImoutoRebirth.Room.Application.Cqrs.FoldersSlice;

public record DestinationFolderQuery(Guid CollectionId) : IQuery<DestinationFolderInfo?>;

public record DestinationFolderInfo(
    Guid Id,
    Guid CollectionId,
    string Path,
    bool ShouldCreateSubfoldersByHash,
    bool ShouldRenameByHash,
    string FormatErrorSubfolder,
    string HashErrorSubfolder,
    string WithoutHashErrorSubfolder);

internal class DestinationFolderQueryHandler : IQueryHandler<DestinationFolderQuery, DestinationFolderInfo?>
{
    private readonly ICollectionRepository _collectionRepository;

    public DestinationFolderQueryHandler(ICollectionRepository collectionRepository) 
        => _collectionRepository = collectionRepository;

    public async Task<DestinationFolderInfo?> Handle(DestinationFolderQuery request, CancellationToken ct)
    {
        var collection = await _collectionRepository.GetById(request.CollectionId).GetAggregateOrThrow();

        if (collection.DestinationFolder.IsDefault())
            return null;
        
        return new(
            collection.DestinationFolder.Id,
            collection.Id,
            collection.DestinationFolder.DestinationDirectory!.FullName,
            collection.DestinationFolder.ShouldCreateSubfoldersByHash,
            collection.DestinationFolder.ShouldRenameByHash,
            collection.DestinationFolder.FormatErrorSubfolder,
            collection.DestinationFolder.HashErrorSubfolder,
            collection.DestinationFolder.WithoutHashErrorSubfolder);
    }
}
