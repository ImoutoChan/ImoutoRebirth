using ImoutoRebirth.Common.Cqrs.Abstract;
using ImoutoRebirth.Room.Application.Cqrs.CollectionFileSlice;
using ImoutoRebirth.Room.DataAccess.Cache;
using ImoutoRebirth.Room.Database;
using Microsoft.EntityFrameworkCore;

namespace ImoutoRebirth.Room.DataAccess.Queries;

internal class FilterCollectionFileHashesQueryHandler(RoomDbContext db, IMd5PresenceCache cache)
    : IQueryHandler<FilterCollectionFileHashesQuery, IReadOnlyCollection<string>>
{
    public Task<IReadOnlyCollection<string>> Handle(FilterCollectionFileHashesQuery request, CancellationToken ct)
        => cache.FindPresent(request.Md5Hashes, LoadPresentHashes, ct);

    private async Task<IReadOnlyCollection<string>> LoadPresentHashes(
        IReadOnlyCollection<string> hashes,
        CancellationToken ct)
        => await db.CollectionFiles
            .Where(file => hashes.Contains(file.Md5))
            .Select(file => file.Md5)
            .Distinct()
            .ToListAsync(ct);
}
