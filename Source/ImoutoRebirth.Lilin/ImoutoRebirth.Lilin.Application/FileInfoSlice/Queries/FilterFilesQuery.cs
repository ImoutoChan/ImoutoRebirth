using ImoutoRebirth.Common.Cqrs.Abstract;

namespace ImoutoRebirth.Lilin.Application.FileInfoSlice.Queries;

public record FilterFilesQuery(
    IReadOnlyCollection<Guid> FileIds,
    IReadOnlyCollection<TagSearchEntry> TagSearchEntries) : IQuery<IReadOnlyCollection<Guid>>;

public record FilterFilesCountQuery(
    IReadOnlyCollection<Guid> FileIds,
    IReadOnlyCollection<TagSearchEntry> TagSearchEntries) : IQuery<int>;

public record TagSearchEntry(Guid TagId, string? Value, TagSearchScope TagSearchScope);

public enum TagSearchScope
{
    Included,
    Excluded
}
