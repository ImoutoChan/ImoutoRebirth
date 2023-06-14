using ImoutoRebirth.Common.Cqrs.Abstract;
using ImoutoRebirth.Lilin.Application.FileInfoSlice.Queries;
using ImoutoRebirth.Lilin.Core.FileInfoAggregate;
using ImoutoRebirth.Lilin.Core.TagAggregate;
using ImoutoRebirth.Lilin.DataAccess;
using ImoutoRebirth.Lilin.Infrastructure.Mappers;
using Microsoft.EntityFrameworkCore;

namespace ImoutoRebirth.Lilin.Infrastructure.Queries;

internal class PopularUserTagsQueryHandler : IQueryHandler<PopularUserTagsQuery, IReadOnlyCollection<Tag>>
{
    private readonly LilinDbContext _lilinDbContext;

    public PopularUserTagsQueryHandler(LilinDbContext lilinDbContext) => _lilinDbContext = lilinDbContext;

    public async Task<IReadOnlyCollection<Tag>> Handle(PopularUserTagsQuery request, CancellationToken ct)
    {
        var tags = _lilinDbContext.FileTags
            .Include(x => x.Tag)
            .ThenInclude(x => x!.Type)
            .Where(x => x.Source == MetadataSource.Manual && x.Tag!.Type!.Name == "General")
            .GroupBy(x => new { x.TagId } )
            .Select(x => new { x.Key.TagId, Count = x.Count() })
            .OrderByDescending(x => x.Count)
            .Take(request.Limit)
            .Select(x => x.TagId);

        var result = await _lilinDbContext.Tags
            .Include(x => x.Type)
            .Where(x => tags.Contains(x.Id)).ToListAsync(cancellationToken: ct);

        return result.Select(x => x.ToModel()).ToArray();
    }
}
