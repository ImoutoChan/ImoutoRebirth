using ImoutoRebirth.Room.Domain;

namespace ImoutoRebirth.Room.Application;

public interface ICollectionFileRepository
{
    Task Add(CollectionFile collectionFile);

    Task<bool> AnyWithPath(
        Guid collectionId,
        string path,
        CancellationToken ct = default);

    Task<IReadOnlyCollection<CollectionFile>> SearchByQuery(
        CollectionFilesQuery query,
        CancellationToken ct = default);

    Task<IReadOnlyCollection<string>> FilterHashesQuery(
        IReadOnlyCollection<string> md5Hashes,
        CancellationToken ct);

    Task<IReadOnlyCollection<Guid>> SearchIdsByQuery(
        CollectionFilesQuery query,
        CancellationToken ct);

    Task<int> CountByQuery(
        CollectionFilesQuery query,
        CancellationToken ct);

    Task Remove(Guid id);

    Task<string?> GetWithMd5(
        Guid collectionId,
        string md5,
        CancellationToken ct = default);
}
