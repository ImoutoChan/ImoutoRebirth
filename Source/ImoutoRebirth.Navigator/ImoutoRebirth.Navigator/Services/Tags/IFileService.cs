using ImoutoRebirth.Navigator.Services.Tags.Model;

namespace ImoutoRebirth.Navigator.Services.Tags;

internal interface IFileService
{
    Task<(IReadOnlyCollection<File> Files, bool Continue)> SearchFiles(
        Guid? collectionId,
        IReadOnlyCollection<SearchTag> tags,
        int take,
        int skip,
        CancellationToken ct);

    Task<int> CountFiles(
        Guid? collectionId, 
        IReadOnlyCollection<SearchTag> tags,
        CancellationToken cancellationToken);

    Task<FileMetadata> GetFileMetadata(Guid fileId);

    Task RemoveFile(Guid fileId);
}
