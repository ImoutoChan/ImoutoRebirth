using ImoutoRebirth.Common.Cqrs.Abstract;
using ImoutoRebirth.Lilin.Domain.TagAggregate;

namespace ImoutoRebirth.Lilin.Application.TagSlice;

public record TagsSearchQuery(string? SearchPattern, int? Limit) : IQuery<IReadOnlyCollection<Tag>>;
