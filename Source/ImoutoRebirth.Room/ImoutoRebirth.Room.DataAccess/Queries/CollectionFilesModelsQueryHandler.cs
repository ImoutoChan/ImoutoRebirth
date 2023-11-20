using ImoutoRebirth.Common.Cqrs.Abstract;
using ImoutoRebirth.Room.Application;
using ImoutoRebirth.Room.Application.Cqrs;
using ImoutoRebirth.Room.DataAccess.Mappers;
using ImoutoRebirth.Room.Database;
using ImoutoRebirth.Room.Database.Entities;
using ImoutoRebirth.Room.Domain;
using Microsoft.EntityFrameworkCore;

namespace ImoutoRebirth.Room.DataAccess.Repositories.Queries;

internal class CollectionFilesModelsQueryHandler 
    : IQueryHandler<CollectionFilesModelsQuery, IReadOnlyCollection<CollectionFile>>
        , IQueryHandler<CollectionFilesIdsQuery, IReadOnlyCollection<Guid>>
        , IQueryHandler<CollectionFilesCountQuery, int>
{
    private readonly RoomDbContext _roomDbContext;

    public CollectionFilesModelsQueryHandler(RoomDbContext roomDbContext) => _roomDbContext = roomDbContext;

    public async Task<IReadOnlyCollection<CollectionFile>> Handle(
        CollectionFilesModelsQuery request,
        CancellationToken ct)
    {
        var query = request.Query;
        
        var files = BuildFilesQuery(query);

        if (query.Skip.HasValue)
            files = files.Skip(query.Skip.Value);

        if (query.Count.HasValue)
            files = files.Take(query.Count.Value);

        var loaded = await files.ToListAsync(cancellationToken: ct);

        return loaded.Select(x => x.ToEntity()).ToList();
    }

    public async Task<IReadOnlyCollection<Guid>> Handle(CollectionFilesIdsQuery request, CancellationToken ct)
    {
        var query = request.Query;
        
        var files = BuildFilesQuery(query);

        if (query.Skip.HasValue)
            files = files.Skip(query.Skip.Value);

        if (query.Count.HasValue)
            files = files.Take(query.Count.Value);

        return await files.Select(x => x.Id).ToListAsync(cancellationToken: ct);
    }

    public async Task<int> Handle(CollectionFilesCountQuery request, CancellationToken ct) 
        => await BuildFilesQuery(request.Query).CountAsync(ct);

    private IQueryable<CollectionFileEntity> BuildFilesQuery(CollectionFilesQuery query)
    {
        var files = _roomDbContext.CollectionFiles.AsQueryable();

        if (query.CollectionId.HasValue)
            files = files.Where(x => x.CollectionId == query.CollectionId.Value);

        if (query.CollectionFileIds?.Any() == true)
            files = files.Where(x => query.CollectionFileIds.Contains(x.Id));

        if (query.Path != null)
            files = files.Where(x => query.Path.Equals(x.Path));

        if (query.Md5 != null && query.Md5.Any())
            files = files.Where(x => query.Md5.Contains(x.Md5));

        files = files.OrderBy(x => x.AddedOn);

        return files;
    }
}
