#nullable enable
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ImoutoViewer.ImoutoRebirth.Services.Tags.Model;

namespace ImoutoRebirth.Navigator.Services.Tags;

interface IFileTagService
{
    Task SetRate(Guid fileId, Rate rate);

    Task SetFavorite(Guid fileId, bool value);

    Task BindTags(IReadOnlyCollection<FileTag> fileTags);

    Task UnbindTag(Guid fileId, Guid tagId, FileTagSource source);

    Task<IReadOnlyCollection<FileTag>> GetFileTags(Guid fileId);
}