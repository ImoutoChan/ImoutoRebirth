using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ImoutoRebirth.Navigator.Services.Collections;

public interface ISourceFolderService
{
    Task<IReadOnlyCollection<SourceFolder>> GetSourceFoldersAsync(Guid collectionId);

    Task<SourceFolder> AddSourceFolderAsync(SourceFolder sourceFolder);

    Task<SourceFolder> UpdateSourceFolderAsync(SourceFolder sourceFolder);

    Task DeleteSourceFolderAsync(Guid collectionId, Guid sourceFolderId);
}