using ImoutoRebirth.Room.Application.Services;
using ImoutoRebirth.Room.DataAccess.Cache;
using ImoutoRebirth.Room.DataAccess.Exceptions;
using ImoutoRebirth.Room.DataAccess.Mappers;
using ImoutoRebirth.Room.Database;
using ImoutoRebirth.Room.Database.Entities;
using ImoutoRebirth.Room.Domain;
using Microsoft.EntityFrameworkCore;

namespace ImoutoRebirth.Room.DataAccess.Repositories;

internal class CollectionFileRepository : ICollectionFileRepository
{
    private readonly RoomDbContext _roomDbContext;
    private readonly Md5PresenceCacheInvalidator _md5PresenceCacheInvalidator;

    public CollectionFileRepository(RoomDbContext roomDbContext, Md5PresenceCacheInvalidator md5PresenceCacheInvalidator)
    {
        _roomDbContext = roomDbContext;
        _md5PresenceCacheInvalidator = md5PresenceCacheInvalidator;
    }

    public async Task<CollectionFile?> GetById(Guid id)
    {
        var file = await _roomDbContext.CollectionFiles
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id);

        return file?.ToModel();
    }

    public async Task Create(CollectionFile collectionFile)
    {
        var entity = collectionFile.ToEntity();

        await _roomDbContext.CollectionFiles.AddAsync(entity);
        await _roomDbContext.SaveChangesAsync();

        _md5PresenceCacheInvalidator.FileChanged(_roomDbContext, entity.Md5);
    }

    public async Task Update(CollectionFile updates)
    {
        var file = await _roomDbContext.CollectionFiles.FindAsync(updates.Id);

        if (file == null)
            throw new EntityNotFoundException<CollectionFileEntity>(updates.Id);

        file.Path = updates.Path;

        await _roomDbContext.SaveChangesAsync();
    }

    public async Task<IReadOnlyCollection<string>> FilterOutExistingPaths(
        Guid collectionId,
        IReadOnlyCollection<string> inputPaths,
        CancellationToken ct = default)
    {
        var existing = await _roomDbContext.CollectionFiles
            .Where(x => x.CollectionId == collectionId && inputPaths.Contains(x.Path))
            .Select(x => x.Path)
            .ToListAsync(ct);

        return inputPaths.Except(existing).ToArray();
    }

    public async Task Remove(Guid id)
    {
        var file = await _roomDbContext.CollectionFiles.FindAsync(id);

        if (file == null)
            throw new EntityNotFoundException<CollectionFileEntity>(id);

        file.IsRemoved = true;

        await _roomDbContext.SaveChangesAsync();

        _md5PresenceCacheInvalidator.FileChanged(_roomDbContext, file.Md5);
    }

    public Task<bool> HasOtherReference(Guid fileId, string path, CancellationToken ct)
        => _roomDbContext.CollectionFiles.AnyAsync(x => x.Id != fileId && x.Path.ToUpper() == path.ToUpper(), ct);

    public Task<string?> GetWithMd5(Guid collectionId, string md5, string path, CancellationToken ct)
        => _roomDbContext.CollectionFiles.IgnoreQueryFilters()
            .Where(x => x.CollectionId == collectionId && x.Md5 == md5)
            .Where(x => !x.IsRemoved ||
                (x.Path.ToUpper() == path.ToUpper() && _roomDbContext.CollectionFiles.Any(other =>
                    !other.IsRemoved && other.CollectionId != collectionId && other.Path.ToUpper() == x.Path.ToUpper())))
            .OrderBy(x => x.IsRemoved)
            .Select(x => x.Path)
            .FirstOrDefaultAsync(ct);
}
