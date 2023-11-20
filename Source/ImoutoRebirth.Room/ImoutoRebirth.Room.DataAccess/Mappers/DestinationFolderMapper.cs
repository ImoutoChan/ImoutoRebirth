using ImoutoRebirth.Room.Database.Entities;
using ImoutoRebirth.Room.Domain.CollectionAggregate;

namespace ImoutoRebirth.Room.DataAccess.Mappers;

internal static class DestinationFolderMapper
{
    public static DestinationFolderEntity? ToEntity(this DestinationFolder destinationFolder, Guid collectionId)
        => destinationFolder.DestinationDirectory == null 
            ? null 
            : new DestinationFolderEntity
        {
            Id = destinationFolder.Id,
            Path = destinationFolder.DestinationDirectory.FullName,
            CollectionId = collectionId,
            WithoutHashErrorSubfolder = destinationFolder.WithoutHashErrorSubfolder,
            ShouldCreateSubfoldersByHash = destinationFolder.ShouldCreateSubfoldersByHash,
            HashErrorSubfolder = destinationFolder.HashErrorSubfolder,
            ShouldRenameByHash = destinationFolder.ShouldRenameByHash,
            FormatErrorSubfolder = destinationFolder.FormatErrorSubfolder
        };

    public static DestinationFolder ToModel(this DestinationFolderEntity? destinationFolder)
        => destinationFolder == null
            ? DestinationFolder.Default
            : new(
                destinationFolder.Id,
                destinationFolder.Path,
                destinationFolder.ShouldCreateSubfoldersByHash,
                destinationFolder.ShouldRenameByHash,
                destinationFolder.FormatErrorSubfolder,
                destinationFolder.HashErrorSubfolder,
                destinationFolder.WithoutHashErrorSubfolder);
}
