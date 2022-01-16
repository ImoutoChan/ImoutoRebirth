using ImoutoRebirth.Room.DataAccess.Models;
using ImoutoRebirth.Room.Database.Entities;

namespace ImoutoRebirth.Room.DataAccess.Mappers;

public static class DestinationFolderMapper
{
    public static DestinationFolderEntity ToEntity(this DestinationFolderCreateData createData)
        => new DestinationFolderEntity
        {
            Id = Guid.NewGuid(),
            Path = createData.Path,
            CollectionId = createData.CollectionId,
            WithoutHashErrorSubfolder = createData.WithoutHashErrorSubfolder,
            ShouldCreateSubfoldersByHash = createData.ShouldCreateSubfoldersByHash,
            HashErrorSubfolder = createData.HashErrorSubfolder,
            ShouldRenameByHash = createData.ShouldRenameByHash,
            FormatErrorSubfolder = createData.FormatErrorSubfolder
        };

    public static DestinationFolderEntity ToEntity(
        this DestinationFolderCreateData createData, 
        DestinationFolderEntity updateCurrent)
    {
        updateCurrent.Path = createData.Path;
        updateCurrent.WithoutHashErrorSubfolder = createData.WithoutHashErrorSubfolder;
        updateCurrent.ShouldCreateSubfoldersByHash = createData.ShouldCreateSubfoldersByHash;
        updateCurrent.HashErrorSubfolder = createData.HashErrorSubfolder;
        updateCurrent.ShouldRenameByHash = createData.ShouldRenameByHash;
        updateCurrent.FormatErrorSubfolder = createData.FormatErrorSubfolder;

        return updateCurrent;
    }
}