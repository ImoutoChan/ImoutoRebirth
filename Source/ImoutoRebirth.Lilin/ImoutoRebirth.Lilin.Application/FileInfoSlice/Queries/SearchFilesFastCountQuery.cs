using ImoutoRebirth.Common.Cqrs.Abstract;

namespace ImoutoRebirth.Lilin.Application.FileInfoSlice.Queries;

public record SearchFilesFastCountQuery(IReadOnlyCollection<TagSearchEntry> TagSearchEntries) : IQuery<int>;
