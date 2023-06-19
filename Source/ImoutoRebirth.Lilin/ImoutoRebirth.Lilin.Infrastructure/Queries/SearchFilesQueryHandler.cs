using ImoutoRebirth.Common.Cqrs.Abstract;
using ImoutoRebirth.Lilin.Application.FileInfoSlice.Queries;
using ImoutoRebirth.Lilin.DataAccess;
using Microsoft.EntityFrameworkCore;

namespace ImoutoRebirth.Lilin.Infrastructure.Queries;

internal class SearchFilesQueryHandler : IQueryHandler<SearchFilesQuery, IReadOnlyCollection<Guid>>
{
    private readonly LilinDbContext _lilinDbContext;

    public SearchFilesQueryHandler(LilinDbContext lilinDbContext) => _lilinDbContext = lilinDbContext;

    public async Task<IReadOnlyCollection<Guid>> Handle(SearchFilesQuery request, CancellationToken ct)
    {
        var (tagSearchEntries, limit, offset) = request;
        
        var filteredFiles = _lilinDbContext
            .GetSearchFilesQueryable(tagSearchEntries)
            .GroupBy(x => x.FileId)
            .Select(x => new { FileId = x.Key, FirstAppeared = x.Min(y => y.AddedOn) })
            .Distinct()
            .OrderBy(x => x.FirstAppeared);

        var filteredFileIds = filteredFiles.Select(x => x.FileId);

        filteredFileIds = filteredFileIds.Skip(offset);

        if (limit.HasValue)
            filteredFileIds = filteredFileIds.Take(limit.Value);

        return await filteredFileIds.ToListAsync(cancellationToken: ct);
    }
}
