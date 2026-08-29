using ImoutoRebirth.Common.Cqrs.Abstract;
using ImoutoRebirth.Lilin.Domain.TagAggregate;

namespace ImoutoRebirth.Lilin.Application.TagSlice;

public record TagAliasesQuery(Guid TagId) : IQuery<IReadOnlyCollection<Tag>>;
