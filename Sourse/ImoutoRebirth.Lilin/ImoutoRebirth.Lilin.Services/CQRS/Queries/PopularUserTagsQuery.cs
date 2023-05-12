using ImoutoRebirth.Common.Cqrs.Abstract;
using ImoutoRebirth.Lilin.Core.Models;

namespace ImoutoRebirth.Lilin.Services.CQRS.Queries;

public record PopularUserTagsQuery(int Limit = 10) : IQuery<IReadOnlyCollection<Tag>>;
