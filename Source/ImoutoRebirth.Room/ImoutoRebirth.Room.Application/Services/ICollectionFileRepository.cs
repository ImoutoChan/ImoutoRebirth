using ImoutoRebirth.Room.Domain;

namespace ImoutoRebirth.Room.Application.Services;

public interface ICollectionFileRepository
{
    Task Create(CollectionFile collectionFile);

    Task<bool> AnyWithPath(
        Guid collectionId,
        string path,
        CancellationToken ct = default);

    Task Remove(Guid id);

    Task<string?> GetWithMd5(
        Guid collectionId,
        string md5,
        CancellationToken ct = default);
}
