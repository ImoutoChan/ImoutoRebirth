using ImoutoRebirth.Room.Domain;

namespace ImoutoRebirth.Room.Application.Services;

public interface ICollectionFileRepository
{
    Task Create(CollectionFile collectionFile);

    Task<IReadOnlyCollection<string>> FilterOutExistingPaths(
        Guid collectionId,
        IReadOnlyCollection<string> inputPaths,
        CancellationToken ct = default);

    Task Remove(Guid id);

    Task<string?> GetWithMd5(
        Guid collectionId,
        string md5,
        CancellationToken ct = default);
}
