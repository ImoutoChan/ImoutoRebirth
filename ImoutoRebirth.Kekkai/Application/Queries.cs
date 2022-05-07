using ImoutoRebirth.Common.Cqrs.Abstract;

namespace ImoutoRebirth.Kekkai.Application;

public record FilesStatusesQuery(IReadOnlyCollection<string> Hashes) : IStreamQuery<FileStatusResult>;

public record FileStatusResult(string Hash, FileStatus Status);

public enum FileStatus
{
    NotFound,
    Present,
    RelativePresent
}
