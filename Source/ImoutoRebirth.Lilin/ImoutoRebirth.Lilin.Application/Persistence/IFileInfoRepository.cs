using ImoutoRebirth.Lilin.Core.FileInfoAggregate;

namespace ImoutoRebirth.Lilin.Application.Persistence;

public interface IFileInfoRepository
{
    Task<FileInfo> Get(Guid fileId, CancellationToken ct);

    Task Save(FileInfo file);
}
