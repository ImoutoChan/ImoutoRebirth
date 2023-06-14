using ImoutoRebirth.Common.Cqrs.Abstract;
using ImoutoRebirth.Lilin.Application.FileInfoSlice.Queries;
using ImoutoRebirth.Lilin.DataAccess;
using Microsoft.EntityFrameworkCore;
using NinjaNye.SearchExtensions;

namespace ImoutoRebirth.Lilin.Infrastructure.Queries;

internal class RelativesBatchQueryHandler : IQueryHandler<RelativesBatchQuery, IReadOnlyCollection<RelativeShortInfo>>
{
    private readonly LilinDbContext _lilinDbContext;

    public RelativesBatchQueryHandler(LilinDbContext lilinDbContext) => _lilinDbContext = lilinDbContext;

    public async Task<IReadOnlyCollection<RelativeShortInfo>> Handle(
        RelativesBatchQuery query,
        CancellationToken ct)
    {
        var hashes = query.Md5;
        
        var request = _lilinDbContext.FileTags
            .Where(x => x.Tag!.Name == "ParentMd5" || x.Tag.Name == "Child")
            .Search(x => x.Value).Containing(hashes.ToArray())
            .Select(x => new
            {
                Value = x.Value,
                TagName = x.Tag!.Name
            });
        
        var fileTags = await request.ToListAsync(cancellationToken: ct);

        return hashes
            .Select(x =>
            {
                var tags = fileTags.Where(y => y.Value?.Contains(x) == true);

                if (tags.Any(y => y.TagName == "ParentMd5"))
                    return (x, RelativeType.Parent);

                if (tags.Any(y => y.TagName == "Child"))
                    return (x, RelativeType.Child);

                return (x, (RelativeType?)null);
            })
            .Select(x => new RelativeShortInfo(x.x, x.Item2))
            .ToList();
    }
}
