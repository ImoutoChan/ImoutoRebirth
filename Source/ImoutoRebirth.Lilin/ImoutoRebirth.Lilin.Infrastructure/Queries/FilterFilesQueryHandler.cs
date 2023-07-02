using ImoutoRebirth.Common.Cqrs.Abstract;
using ImoutoRebirth.Lilin.Application.FileInfoSlice.Queries;
using ImoutoRebirth.Lilin.DataAccess;
using Microsoft.EntityFrameworkCore;

namespace ImoutoRebirth.Lilin.Infrastructure.Queries;

internal class FilterFilesQueryHandler 
    : IQueryHandler<FilterFilesQuery, IReadOnlyCollection<Guid>>
    , IQueryHandler<FilterFilesCountQuery, int>
{
    private readonly LilinDbContext _lilinDbContext;

    public FilterFilesQueryHandler(LilinDbContext lilinDbContext) => _lilinDbContext = lilinDbContext;

    public async Task<IReadOnlyCollection<Guid>> Handle(FilterFilesQuery request, CancellationToken ct)
    {
        var (fileIds, tagSearchEntries) = request;

        if (tagSearchEntries.All(x => x.TagSearchScope == TagSearchScope.Excluded))
        {
            var invertedTagSearchEntries = tagSearchEntries
                .Select(x => x with { TagSearchScope = TagSearchScope.Included })
                .ToArray();

            var allFilesWithAnyInvertedTagQuery = _lilinDbContext.GetSearchFilesQueryable(
                    invertedTagSearchEntries,
                    SearchOptions.FileShouldHaveAnyIncludedTags)
                .Select(x => x.FileId)
                .Distinct();
            
            var allFilesWithAnyInvertedTag = await allFilesWithAnyInvertedTagQuery.ToListAsync(ct);
            
            return fileIds.Except(allFilesWithAnyInvertedTag).ToList();
        }
        
        return await _lilinDbContext.GetSearchFilesQueryable(tagSearchEntries)
            .Where(x => fileIds.Contains(x.FileId))
            .Select(x => x.FileId)
            .Distinct()
            .ToListAsync(ct);
    }

    public async Task<int> Handle(FilterFilesCountQuery request, CancellationToken ct)
    {
        var (fileIds, tagSearchEntries) = request;

        if (tagSearchEntries.All(x => x.TagSearchScope == TagSearchScope.Excluded))
        {
            var invertedTagSearchEntries = tagSearchEntries
                .Select(x => x with { TagSearchScope = TagSearchScope.Included })
                .ToArray();

            var allFilesWithAnyInvertedTagQuery = _lilinDbContext.GetSearchFilesQueryable(
                    invertedTagSearchEntries,
                    SearchOptions.FileShouldHaveAnyIncludedTags)
                .Select(x => x.FileId)
                .Distinct();
            
            var allFilesWithAnyInvertedTag = await allFilesWithAnyInvertedTagQuery.ToListAsync(ct);
            
            return fileIds.Except(allFilesWithAnyInvertedTag).Count();
        }
        
        return await _lilinDbContext.GetSearchFilesQueryable(tagSearchEntries)
            .Where(x => fileIds.Contains(x.FileId))
            .Select(x => x.FileId)
            .Distinct()
            .CountAsync(ct);
    }
}
