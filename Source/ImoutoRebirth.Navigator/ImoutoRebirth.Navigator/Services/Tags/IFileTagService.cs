using ImoutoRebirth.Navigator.Services.Tags.Model;

namespace ImoutoRebirth.Navigator.Services.Tags;

interface IFileTagService
{
    Task SetRate(Guid fileId, Rate rate);

    Task SetFavorite(Guid fileId, bool value);

    Task BindTags(IReadOnlyCollection<FileTag> fileTags);

    Task UnbindTags(params UnbindTagRequest[] tagsToUnbind);

    Task<IReadOnlyCollection<FileTag>> GetFileTags(Guid fileId);
}

record UnbindTagRequest(Guid FileId, Guid TagId, string? Value, FileTagSource Source);
