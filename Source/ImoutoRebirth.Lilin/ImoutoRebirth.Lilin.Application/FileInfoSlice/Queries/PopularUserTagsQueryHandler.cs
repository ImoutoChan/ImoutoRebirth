using ImoutoRebirth.Common.Cqrs.Abstract;
using ImoutoRebirth.Lilin.Core.TagAggregate;

namespace ImoutoRebirth.Lilin.Application.FileInfoSlice.Queries;

public record PopularUserTagsQuery(int Limit = 10) : IQuery<IReadOnlyCollection<Tag>>;
