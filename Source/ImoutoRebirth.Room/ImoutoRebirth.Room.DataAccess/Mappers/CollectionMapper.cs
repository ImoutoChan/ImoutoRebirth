using ImoutoRebirth.Room.Database.Entities;
using ImoutoRebirth.Room.Domain.CollectionAggregate;

namespace ImoutoRebirth.Room.DataAccess.Mappers;

internal static class CollectionMapper
{
    public static CollectionEntity ToEntity(this Collection collection)
        => new()
        {
            Id = collection.Id,
            Name = collection.Name,
            DestinationFolder = collection.DestinationFolder.ToEntity(collection.Id),
            SourceFolders = collection.SourceFolders.Select(x => x.ToEntity(collection.Id)).ToList()
        };

    public static Collection ToModel(this CollectionEntity collection)
        => new(
            collection.Id,
            collection.Name,
            collection.DestinationFolder.ToModel(),
            collection.SourceFolders?.Select(x => x.ToModel()).ToArray() ?? Array.Empty<SourceFolder>());
}
