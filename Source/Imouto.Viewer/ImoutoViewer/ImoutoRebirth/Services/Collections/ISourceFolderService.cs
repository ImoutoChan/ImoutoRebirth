namespace ImoutoViewer.ImoutoRebirth.Services.Collections;

public interface ISourceFolderService
{
    Task<IReadOnlyCollection<SourceFolder>> GetSourceFoldersAsync(Guid collectionId);

    Task<SourceFolder> AddSourceFolderAsync(SourceFolder sourceFolder);

    Task<SourceFolder> UpdateSourceFolderAsync(SourceFolder sourceFolder);

    Task DeleteSourceFolderAsync(Guid collectionId, Guid sourceFolderId);
}