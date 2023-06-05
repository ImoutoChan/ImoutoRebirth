#nullable enable
using ImoutoViewer.ImoutoRebirth.Services.Tags.Model;

namespace ImoutoViewer.ImoutoRebirth.Services.Tags;

internal interface IFileTagService
{
    Task SetRate(Guid fileId, Rate rate);

    Task SetFavorite(Guid fileId, bool value);

    Task BindTags(IReadOnlyCollection<FileTag> fileTags);

    Task UnbindTag(Guid fileId, Guid tagId, FileTagSource source);

    Task<IReadOnlyCollection<FileTag>> GetFileTags(Guid fileId);
    
    Task<IReadOnlyCollection<FileNote>> GetFileNotes(Guid fileId);
}
