using ImoutoRebirth.Common.Cqrs.Abstract;
using ImoutoRebirth.Common.Domain;
using ImoutoRebirth.Room.Application.Services;

namespace ImoutoRebirth.Room.Application.Cqrs.FoldersSlice;

public record SourceFoldersQuery(Guid CollectionId) : IQuery<IReadOnlyCollection<SourceFolderInfo>>;

public record SourceFolderInfo(
    Guid Id,
    Guid CollectionId,
    string Path,
    bool ShouldCheckFormat,
    bool ShouldCheckHashFromName,
    bool ShouldCreateTagsFromSubfolders,
    bool ShouldAddTagFromFilename,
    IReadOnlyCollection<string> SupportedExtensions);

internal class SourceFoldersQueryHandler : IQueryHandler<SourceFoldersQuery, IReadOnlyCollection<SourceFolderInfo>>
{
    private readonly ICollectionRepository _collectionRepository;

    public SourceFoldersQueryHandler(ICollectionRepository collectionRepository) 
        => _collectionRepository = collectionRepository;

    public async Task<IReadOnlyCollection<SourceFolderInfo>> Handle(SourceFoldersQuery request, CancellationToken ct)
    {
        var collection = await _collectionRepository.GetById(request.CollectionId).GetAggregateOrThrow();

        return collection.SourceFolders
            .Select(
                x => new SourceFolderInfo(
                    x.Id,
                    collection.Id,
                    x.Path,
                    x.ShouldCheckFormat,
                    x.ShouldCheckHashFromName,
                    x.ShouldCreateTagsFromSubfolders,
                    x.ShouldAddTagFromFilename,
                    x.SupportedExtensions))
            .ToList();
    }
}
