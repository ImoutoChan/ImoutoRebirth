using ImoutoRebirth.Common.Cqrs.Abstract;
using ImoutoRebirth.Lilin.Application.FileInfoSlice.Queries;
using ImoutoRebirth.Lilin.DataAccess;
using Microsoft.EntityFrameworkCore;

namespace ImoutoRebirth.Lilin.Infrastructure.Queries;

internal class SearchFilesQueryCountHandler : IQueryHandler<SearchFilesCountQuery, int>
{
    private readonly LilinDbContext _lilinDbContext;

    public SearchFilesQueryCountHandler(LilinDbContext lilinDbContext) => _lilinDbContext = lilinDbContext;
    
    public Task<int> Handle(SearchFilesCountQuery request, CancellationToken ct)
    {
        return _lilinDbContext.GetSearchFilesQueryable(request.TagSearchEntries)
            .GroupBy(x => x.FileId)
            .Select(x => x.Key)
            .Distinct()
            .CountAsync(cancellationToken: ct);
    }
}
