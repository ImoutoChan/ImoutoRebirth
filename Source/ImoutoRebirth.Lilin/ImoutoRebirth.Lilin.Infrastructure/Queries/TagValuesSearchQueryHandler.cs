using ImoutoRebirth.Common;
using ImoutoRebirth.Common.Cqrs.Abstract;
using ImoutoRebirth.Lilin.Application.TagSlice;
using ImoutoRebirth.Lilin.DataAccess;
using Microsoft.EntityFrameworkCore;

namespace ImoutoRebirth.Lilin.Infrastructure.Queries;

internal class TagValuesSearchQueryHandler : IQueryHandler<TagValuesSearchQuery, IReadOnlyCollection<string>>
{
    private const int DefaultLimit = 10;
    
    private readonly LilinDbContext _lilinDbContext;

    public TagValuesSearchQueryHandler(LilinDbContext lilinDbContext) => _lilinDbContext = lilinDbContext;

    public async Task<IReadOnlyCollection<string>> Handle(TagValuesSearchQuery query, CancellationToken ct)
    {
        var (tagId, searchPattern, requestedLimit) = query;

        var limit = requestedLimit ?? DefaultLimit;

        var fileTags = _lilinDbContext.FileTags
            .Where(x => x.TagId == tagId)
            .Select(x => x.Value)
            .WhereNotNull()
            .Distinct()
            .OrderBy(x => x);

        if (string.IsNullOrWhiteSpace(searchPattern))
        {
            return (await fileTags.Take(limit).ToListAsync(cancellationToken: ct))!;
        }

        searchPattern = searchPattern.ToLower();

        var finalResult = await fileTags
            .Where(x => x.ToLower().Equals(searchPattern))
            .Take(limit)
            .ToListAsync(cancellationToken: ct);

        if (finalResult.Count < limit)
        {
            var limitLeft = limit - finalResult.Count;

            var startsWith = await fileTags
                .Where(x => !x.ToLower().Equals(searchPattern))
                .Where(x => x.ToLower().StartsWith(searchPattern))
                .Take(limitLeft)
                .ToListAsync(cancellationToken: ct);

            finalResult.AddRange(startsWith);
        }

        if (finalResult.Count < limit)
        {
            var limitLeft = limit - finalResult.Count;

            var contains = await fileTags
                .Where(x => !x.ToLower().Equals(searchPattern))
                .Where(x => !x.ToLower().StartsWith(searchPattern))
                .Where(x => x.ToLower().Contains(searchPattern))
                .Take(limitLeft)
                .ToListAsync(cancellationToken: ct);

            finalResult.AddRange(contains);
        }

        return finalResult;
    }
}
