using ImoutoRebirth.Room.Database.Entities;
using ImoutoRebirth.Room.Domain.CollectionAggregate;

namespace ImoutoRebirth.Room.DataAccess.Mappers;

internal static class SourceFolderMapper
{
    public static SourceFolderEntity ToEntity(this SourceFolder sourceFolder, Guid collectionId) => new()
    {
        Id = sourceFolder.Id,
        CollectionId = collectionId,
        Path = sourceFolder.Path,
        ShouldCheckFormat = sourceFolder.ShouldCheckFormat,
        ShouldCheckHashFromName = sourceFolder.ShouldCheckHashFromName,
        ShouldCreateTagsFromSubfolders = sourceFolder.ShouldCreateTagsFromSubfolders,
        ShouldAddTagFromFilename = sourceFolder.ShouldAddTagFromFilename,
        SupportedExtensionCollection = sourceFolder.SupportedExtensions,
        IsWebhookUploadEnabled = sourceFolder.IsWebhookUploadEnabled,
        WebhookUploadUrl = sourceFolder.WebhookUploadUrl
    };

    public static SourceFolder ToModel(this SourceFolderEntity sourceFolder) =>
        new(sourceFolder.Id,
            sourceFolder.Path,
            sourceFolder.ShouldCheckFormat,
            sourceFolder.ShouldCheckHashFromName,
            sourceFolder.ShouldCreateTagsFromSubfolders,
            sourceFolder.ShouldAddTagFromFilename,
            sourceFolder.SupportedExtensionCollection ?? [],
            sourceFolder.IsWebhookUploadEnabled,
            sourceFolder.WebhookUploadUrl);
}
