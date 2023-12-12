using ImoutoRebirth.Common.Cqrs.Abstract;
using ImoutoRebirth.Lilin.Domain.TagAggregate;

namespace ImoutoRebirth.Lilin.Application.FileInfoSlice.Queries;

public record PopularUserTagsQuery(int Limit = 10) : IQuery<IReadOnlyCollection<Tag>>;

public record PopularUserCharacterTagsQuery(int Limit = 10) : IQuery<IReadOnlyCollection<Tag>>;
