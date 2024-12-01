using ImoutoRebirth.Common.Cqrs.Abstract;
using ImoutoRebirth.Room.Application.Cqrs.CollectionFileSlice;
using ImoutoRebirth.Room.Database;
using Microsoft.EntityFrameworkCore;

namespace ImoutoRebirth.Room.DataAccess.Queries;

internal class CollectionFileMetadataQueryHandler
    : IQueryHandler<CollectionFileMetadataQuery, CollectionFileMetadata?>
{
    private readonly RoomDbContext _roomDbContext;

    public CollectionFileMetadataQueryHandler(RoomDbContext roomDbContext)
        => _roomDbContext = roomDbContext;

    public async Task<CollectionFileMetadata?> Handle(CollectionFileMetadataQuery query, CancellationToken ct)
    {
        var id = query.Id;

        var file = await _roomDbContext.CollectionFiles.FirstOrDefaultAsync(x => x.Id == id, ct);

        if (file == null)
            return null;

        return new CollectionFileMetadata(file.Id, file.Md5, file.AddedOn);
    }
}
