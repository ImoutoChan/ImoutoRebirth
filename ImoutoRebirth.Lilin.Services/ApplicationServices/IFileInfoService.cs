using ImoutoRebirth.Lilin.Core.Models.FileInfoAggregate;

namespace ImoutoRebirth.Lilin.Services.ApplicationServices;

public interface IFileInfoService
{
    Task<FileInfo> LoadFileAggregate(Guid fileId, CancellationToken ct);

    Task PersistFileAggregate(FileInfo file);
}
