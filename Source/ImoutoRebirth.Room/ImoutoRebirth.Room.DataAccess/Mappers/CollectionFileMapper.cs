using ImoutoRebirth.Room.Database.Entities;
using ImoutoRebirth.Room.Domain;

namespace ImoutoRebirth.Room.DataAccess.Mappers;

public static class CollectionFileMapper
{
    public static CollectionFileEntity ToEntity(this CollectionFile file)
        => new()
        {
            Id = file.Id,
            CollectionId = file.CollectionId,
            Path = file.Path,
            Md5 = file.Md5,
            Size = file.Size,
            OriginalPath = file.OriginalPath
        };

    public static CollectionFile ToEntity(this CollectionFileEntity file)
        => new(
            file.Id,
            file.CollectionId,
            file.Path,
            file.Md5,
            file.Size,
            file.OriginalPath!);
}