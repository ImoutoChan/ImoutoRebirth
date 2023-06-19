using ImoutoRebirth.Common.Cqrs.Abstract;
using ImoutoRebirth.Lilin.Application.FileInfoSlice.Queries;
using ImoutoRebirth.Lilin.DataAccess;
using Microsoft.EntityFrameworkCore;

namespace ImoutoRebirth.Lilin.Infrastructure.Queries;

internal class SearchFilesFastQueryHandler 
    : IQueryHandler<SearchFilesFastQuery, IReadOnlyCollection<Guid>>
    , IQueryHandler<SearchFilesFastCountQuery, int>
{
    private readonly LilinDbContext _lilinDbContext;

    public SearchFilesFastQueryHandler(LilinDbContext lilinDbContext) => _lilinDbContext = lilinDbContext;

    public async Task<IReadOnlyCollection<Guid>> Handle(SearchFilesFastQuery request, CancellationToken ct)
    {
        var tagSearchEntries = request.TagSearchEntries;
        var queryable = BuildQueryable(tagSearchEntries);
        return await queryable.ToListAsync(cancellationToken: ct);
    }

    public async Task<int> Handle(SearchFilesFastCountQuery request, CancellationToken ct)
    {
        var tagSearchEntries = request.TagSearchEntries;
        var queryable = BuildQueryable(tagSearchEntries);
        return await queryable.CountAsync(cancellationToken: ct);
    }

    private IQueryable<Guid> BuildQueryable(IReadOnlyCollection<TagSearchEntry> tagSearchEntries)
    {
        IQueryable<Guid>? queryable;

        var include = tagSearchEntries.Where(x => x.TagSearchScope == TagSearchScope.Included).ToList();
        var exclude = tagSearchEntries.Where(x => x.TagSearchScope == TagSearchScope.Excluded).ToList();

        if (include.Any())
        {
            var includeFirst = include.First();

            queryable = _lilinDbContext.FileTags
                .Where(x => x.TagId == includeFirst.TagId)
                .MakeValueFilter(includeFirst.Value)
                .Select(x => x.FileId);

            foreach (var includeEntry in include.Skip(1))
            {
                queryable = queryable.Intersect(
                    _lilinDbContext.FileTags
                        .Where(x => x.TagId == includeEntry.TagId)
                        .MakeValueFilter(includeEntry.Value)
                        .Select(x => x.FileId)
                );
            }
        }
        else
        {
            queryable = _lilinDbContext.FileTags.Select(x => x.FileId);
        }

        foreach (var excludeEntry in exclude)
        {
            queryable = queryable.Except(
                _lilinDbContext.FileTags
                    .Where(x => x.TagId == excludeEntry.TagId)
                    .MakeValueFilter(excludeEntry.Value)
                    .Select(x => x.FileId)
            );
        }

        return queryable;
    }
}
