using ImoutoRebirth.Lilin.Core.Models;
using ImoutoRebirth.Lilin.Core.Models.FileInfoAggregate;

namespace ImoutoRebirth.Lilin.Core.Infrastructure;

public interface IFileTagRepository
{
    Task<List<(string x, RelativeType?)>> SearchHashesInTags(IReadOnlyCollection<string> hashes, CancellationToken ct);
    
    Task<Guid[]> SearchFiles(
        IReadOnlyCollection<TagSearchEntry> tagSearchEntries,
        int? limit,
        int offset,
        CancellationToken ct = default);

    Task<int> SearchFilesCount(IReadOnlyCollection<TagSearchEntry> tagSearchEntries, CancellationToken ct);

    Task<IReadOnlyCollection<FileTag>> GetForFile(Guid fileId, CancellationToken ct);

    Task<IReadOnlyCollection<Tag>> GetPopularUserTagIds(int requestLimit, CancellationToken ct);
    
    Task<Guid[]> FilterFiles(
        IReadOnlyCollection<TagSearchEntry> tagSearchEntries, 
        IReadOnlyCollection<Guid> fileIds, 
        CancellationToken ct);

    Task Add(FileTag fileTag);

    Task AddBatch(IReadOnlyCollection<FileTagInfo> fileTags);

    Task Delete(FileTag fileTag);
}
