namespace ImoutoRebirth.Navigator.Services.Collections;

public interface ISourceFolderService
{
    Task<IReadOnlyCollection<SourceFolder>> GetSourceFoldersAsync(Guid collectionId);

    Task<SourceFolder> AddSourceFolderAsync(SourceFolder sourceFolder);

    Task UpdateSourceFolderAsync(SourceFolder sourceFolder);

    Task DeleteSourceFolderAsync(Guid collectionId, Guid sourceFolderId);
}