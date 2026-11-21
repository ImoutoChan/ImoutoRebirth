using ImoutoViewer.ImoutoRebirth.Services.Tags.Model;

namespace ImoutoViewer.ImoutoRebirth.Services.Tags;

internal interface IFileTagService
{
    Task SetRate(Guid fileId, Rate rate);

    Task SetFavorite(Guid fileId, bool value);

    Task BindTags(IReadOnlyCollection<FileTag> fileTags);

    Task UnbindTags(params UnbindTagRequest[] tagsToUnbind);

    Task<(IReadOnlyCollection<FileTag> Tags, IReadOnlyCollection<FileNote> Notes)> GetFileMetadata(Guid fileId, CancellationToken ct);
}

internal record UnbindTagRequest(Guid FileId, Guid TagId, string? Value, FileTagSource Source);
