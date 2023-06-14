using ImoutoRebirth.Common.Cqrs.Abstract;

namespace ImoutoRebirth.Lilin.Application.FileInfoSlice.Queries;

public record SearchFilesQueryCount(IReadOnlyCollection<TagSearchEntry> TagSearchEntries) : IQuery<int>;

