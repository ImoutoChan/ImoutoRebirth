namespace ImoutoRebirth.Room.DataAccess.Repositories.Queries;

// ReSharper disable once ClassNeverInstantiated.Global
// Automapper
public class CollectionFilesQuery
{
    public Guid? CollectionId { get; }

    public IReadOnlyCollection<Guid> CollectionFileIds { get; }

    public string? Path { get; }

    public IReadOnlyCollection<string>? Md5 { get; }

    public int? Count { get; }

    public int? Skip { get; }

    public CollectionFilesQuery(
        Guid? collectionId,
        IReadOnlyCollection<Guid> collectionFileIds,
        string? path,
        IReadOnlyCollection<string> md5,
        int? count,
        int? skip)
    {
        CollectionId = collectionId;
        CollectionFileIds = collectionFileIds;
        Path = path;
        Md5 = md5?.Select(x => x.ToLowerInvariant()).ToArray();
        Count = count;
        Skip = skip;
    }
}