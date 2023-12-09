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
        var queryable = _lilinDbContext.GetSearchFilesIdsQueryable(tagSearchEntries);
        return await queryable.ToListAsync(cancellationToken: ct);
    }

    public async Task<int> Handle(SearchFilesFastCountQuery request, CancellationToken ct)
    {
        var tagSearchEntries = request.TagSearchEntries;
        var queryable = _lilinDbContext.GetSearchFilesIdsQueryable(tagSearchEntries);
        return await queryable.CountAsync(cancellationToken: ct);
    }
}
