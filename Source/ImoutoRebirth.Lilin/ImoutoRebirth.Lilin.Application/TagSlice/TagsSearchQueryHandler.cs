using ImoutoRebirth.Common.Cqrs.Abstract;
using ImoutoRebirth.Lilin.Core.TagAggregate;

namespace ImoutoRebirth.Lilin.Application.TagSlice;

public record TagsSearchQuery(string? SearchPattern, int? Limit) : IQuery<IReadOnlyCollection<Tag>>;
