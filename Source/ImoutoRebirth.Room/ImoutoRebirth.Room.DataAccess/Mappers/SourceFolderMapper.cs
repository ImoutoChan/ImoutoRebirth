using ImoutoRebirth.Room.Database.Entities;
using ImoutoRebirth.Room.Domain.CollectionAggregate;

namespace ImoutoRebirth.Room.DataAccess.Mappers;

internal static class SourceFolderMapper
{
    public static SourceFolderEntity ToEntity(this SourceFolder sourceFolder, Guid collectionId)
        => new()
        {
            Id = sourceFolder.Id,
            Path = sourceFolder.Path,
            CollectionId = collectionId,
            ShouldCheckFormat = sourceFolder.ShouldCheckFormat,
            ShouldCheckHashFromName = sourceFolder.ShouldCheckHashFromName,
            ShouldCreateTagsFromSubfolders = sourceFolder.ShouldCreateTagsFromSubfolders,
            ShouldAddTagFromFilename = sourceFolder.ShouldAddTagFromFilename,
            SupportedExtensionCollection = sourceFolder.SupportedExtensions
        };

    public static SourceFolder ToModel(this SourceFolderEntity sourceFolder)
        => new(
            sourceFolder.Id,
            sourceFolder.Path,
            sourceFolder.ShouldCheckFormat,
            sourceFolder.ShouldCheckHashFromName,
            sourceFolder.ShouldCreateTagsFromSubfolders,
            sourceFolder.ShouldAddTagFromFilename,
            sourceFolder.SupportedExtensionCollection ?? Array.Empty<string>());
}
