using ImoutoRebirth.Common.Cqrs.Abstract;

namespace ImoutoRebirth.Kekkai.Application;

public record FilesStatusesQuery(IReadOnlyCollection<string> Hashes) : IQuery<IAsyncEnumerable<FileStatusResult>>;

public record FileStatusResult(string Hash, FileStatus Status);

public enum FileStatus
{
    NotFound,
    Present,
    RelativePresent
}
