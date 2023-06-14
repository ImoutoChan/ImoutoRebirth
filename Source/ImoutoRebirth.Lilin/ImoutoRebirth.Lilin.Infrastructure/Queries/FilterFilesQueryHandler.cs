using ImoutoRebirth.Common.Cqrs.Abstract;
using ImoutoRebirth.Lilin.Application.FileInfoSlice.Queries;
using ImoutoRebirth.Lilin.DataAccess;
using Microsoft.EntityFrameworkCore;

namespace ImoutoRebirth.Lilin.Infrastructure.Queries;

internal class FilterFilesQueryHandler : IQueryHandler<FilterFilesQuery, IReadOnlyCollection<Guid>>
{
    private readonly LilinDbContext _lilinDbContext;

    public FilterFilesQueryHandler(LilinDbContext lilinDbContext) => _lilinDbContext = lilinDbContext;

    public async Task<IReadOnlyCollection<Guid>> Handle(FilterFilesQuery request, CancellationToken ct)
    {
        var (fileIds, tagSearchEntries) = request;

        var withoutTags = new List<Guid>();
        if (tagSearchEntries.All(x => x.TagSearchScope == TagSearchScope.Excluded))
        {
            var allFilesWithTags = await _lilinDbContext.FileTags
                .Where(x => fileIds.Contains(x.FileId))
                .Select(x => x.FileId)
                .ToListAsync(cancellationToken: ct);
            withoutTags = fileIds.Except(allFilesWithTags).ToList();
        }
        
        var withTags = await _lilinDbContext.GetSearchFilesQueryable(tagSearchEntries)
            .Where(x => fileIds.Contains(x.FileId))
            .Select(x => x.FileId)
            .Distinct()
            .ToArrayAsync(ct);

        return withTags
            .Union(withoutTags)
            .ToArray();
    }
}
