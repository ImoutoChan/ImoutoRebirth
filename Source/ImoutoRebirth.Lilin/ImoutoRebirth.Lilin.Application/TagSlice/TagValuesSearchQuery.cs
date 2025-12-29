using ImoutoRebirth.Common.Cqrs.Abstract;

namespace ImoutoRebirth.Lilin.Application.TagSlice;

public record TagValuesSearchQuery(Guid TagId, string? SearchPattern, int? Limit) : IQuery<IReadOnlyCollection<string>>;
