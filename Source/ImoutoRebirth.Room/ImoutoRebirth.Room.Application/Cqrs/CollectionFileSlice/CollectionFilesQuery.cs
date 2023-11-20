using ImoutoRebirth.Common.Cqrs.Abstract;
using ImoutoRebirth.Room.Domain;

namespace ImoutoRebirth.Room.Application.Cqrs.CollectionFileSlice;

public record CollectionFilesQuery
{
    public CollectionFilesQuery(Guid? CollectionId,
        IReadOnlyCollection<Guid> CollectionFileIds,
        string? Path,
        IReadOnlyCollection<string>? Md5,
        int? Count,
        int? Skip)
    {
        this.CollectionId = CollectionId;
        this.CollectionFileIds = CollectionFileIds;
        this.Path = Path;
        this.Md5 = Md5?.Select(x => x.ToLowerInvariant()).ToList();
        this.Count = Count;
        this.Skip = Skip;
    }

    public Guid? CollectionId { get; init; }
    public IReadOnlyCollection<Guid>? CollectionFileIds { get; init; }
    public string? Path { get; init; }
    public IReadOnlyCollection<string>? Md5 { get; init; }
    public int? Count { get; init; }
    public int? Skip { get; init; }
}

public record CollectionFilesModelsQuery(CollectionFilesQuery Query) : IQuery<IReadOnlyCollection<CollectionFile>>;

public record CollectionFilesIdsQuery(CollectionFilesQuery Query) : IQuery<IReadOnlyCollection<Guid>>;

public record CollectionFilesCountQuery(CollectionFilesQuery Query) : IQuery<int>;
