using ImoutoRebirth.Common.Cqrs.Abstract;

namespace ImoutoRebirth.Lilin.Application.FileInfoSlice.Queries;

public record SearchFilesQuery(IReadOnlyCollection<TagSearchEntry> TagSearchEntries, int? Limit, int Offset) 
    : IQuery<IReadOnlyCollection<Guid>>;
