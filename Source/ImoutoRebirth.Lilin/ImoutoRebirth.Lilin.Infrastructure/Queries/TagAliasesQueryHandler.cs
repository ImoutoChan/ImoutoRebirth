using ImoutoRebirth.Common.Cqrs.Abstract;
using ImoutoRebirth.Lilin.Application.TagSlice;
using ImoutoRebirth.Lilin.DataAccess;
using ImoutoRebirth.Lilin.Domain.TagAggregate;
using ImoutoRebirth.Lilin.Infrastructure.Mappers;
using Microsoft.EntityFrameworkCore;

namespace ImoutoRebirth.Lilin.Infrastructure.Queries;

internal class TagAliasesQueryHandler : IQueryHandler<TagAliasesQuery, IReadOnlyCollection<Tag>>
{
    private readonly LilinDbContext _lilinDbContext;

    public TagAliasesQueryHandler(LilinDbContext lilinDbContext) => _lilinDbContext = lilinDbContext;

    public async Task<IReadOnlyCollection<Tag>> Handle(TagAliasesQuery request, CancellationToken ct)
    {
        var tagId = request.TagId;

        var aliasIds = _lilinDbContext.TagAliases
            .Where(x => x.TagId == tagId)
            .Select(x => x.AliasTagId)
            .Union(_lilinDbContext.TagAliases
                .Where(x => x.AliasTagId == tagId)
                .Select(x => x.TagId));

        var tags = await _lilinDbContext.Tags
            .Where(x => aliasIds.Contains(x.Id))
            .Include(x => x.Type)
            .ToListAsync(cancellationToken: ct);

        return tags.Select(x => x.ToModel()).ToArray();
    }
}
