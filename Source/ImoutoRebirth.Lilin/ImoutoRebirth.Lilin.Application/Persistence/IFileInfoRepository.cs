using ImoutoRebirth.Lilin.Domain.FileInfoAggregate;

namespace ImoutoRebirth.Lilin.Application.Persistence;

public interface IFileInfoRepository
{
    Task<FileInfo> Get(Guid fileId, CancellationToken ct);

    Task Save(FileInfo file);
}
