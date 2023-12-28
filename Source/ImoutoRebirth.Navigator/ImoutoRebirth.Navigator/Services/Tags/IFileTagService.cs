using ImoutoRebirth.LilinService.WebApi.Client;
using ImoutoRebirth.Navigator.Services.Tags.Model;

namespace ImoutoRebirth.Navigator.Services.Tags;

internal interface IFileTagService
{
    Task SetRate(Guid fileId, Rate rate);

    Task SetFavorite(Guid fileId, bool value);

    Task SetWasWallpaper(Guid selectedItemDbId);

    Task BindTags(
        IReadOnlyCollection<FileTag> fileTags,
        SameTagHandleStrategy strategy = SameTagHandleStrategy.AddNewFileTag);

    Task UnbindTags(params UnbindTagRequest[] tagsToUnbind);

    Task<IReadOnlyCollection<FileTag>> GetFileTags(Guid fileId);
}

internal record UnbindTagRequest(Guid FileId, Guid TagId, string? Value, FileTagSource Source);
