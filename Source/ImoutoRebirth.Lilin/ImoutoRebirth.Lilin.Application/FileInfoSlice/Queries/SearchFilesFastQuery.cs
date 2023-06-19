using ImoutoRebirth.Common.Cqrs.Abstract;

namespace ImoutoRebirth.Lilin.Application.FileInfoSlice.Queries;

public record SearchFilesFastQuery(IReadOnlyCollection<TagSearchEntry> TagSearchEntries) 
    : IQuery<IReadOnlyCollection<Guid>>;
