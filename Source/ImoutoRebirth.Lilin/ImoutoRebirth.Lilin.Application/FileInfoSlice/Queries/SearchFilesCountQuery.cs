using ImoutoRebirth.Common.Cqrs.Abstract;

namespace ImoutoRebirth.Lilin.Application.FileInfoSlice.Queries;

public record SearchFilesCountQuery(IReadOnlyCollection<TagSearchEntry> TagSearchEntries) : IQuery<int>;
