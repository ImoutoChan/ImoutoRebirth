using ImoutoRebirth.Lilin.Domain.FileInfoAggregate;

namespace ImoutoRebirth.Lilin.Application.Persistence;

public interface IFileTagRepository
{
    Task<IReadOnlyCollection<FileTag>> GetForFile(Guid fileId, CancellationToken ct = default);

    Task Add(FileTag fileTag);

    Task Delete(FileTag fileTag);
}
