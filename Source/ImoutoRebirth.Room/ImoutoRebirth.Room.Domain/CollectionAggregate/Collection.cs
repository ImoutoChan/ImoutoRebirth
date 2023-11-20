namespace ImoutoRebirth.Room.Domain.CollectionAggregate;

public class Collection
{
    public Collection(
        Guid id,
        string name,
        DestinationFolder destinationFolder,
        IReadOnlyCollection<SourceFolder> sourceFolders)
    {
        Id = id;
        Name = name;
        DestinationFolder = destinationFolder;
        SourceFolders = sourceFolders;
    }

    public Guid Id { get; }

    public string Name { get; private set; }

    public DestinationFolder DestinationFolder { get; private set; }

    public IReadOnlyCollection<SourceFolder> SourceFolders { get; private set; }
    
    public static Collection Create(string name)
    {
        return new(
            Guid.NewGuid(),
            name,
            DestinationFolder.Default,
            Array.Empty<SourceFolder>());
    }

    public void Rename(string newName) => Name = newName;

    public void SetDestinationFolder(
        string path,
        bool shouldCreateSubfoldersByHash,
        bool shouldRenameByHash,
        string formatErrorSubfolder,
        string hashErrorSubfolder,
        string withoutHashErrorSubfolder)
    {
        DestinationFolder = new DestinationFolder(
            Guid.NewGuid(),
            path,
            shouldCreateSubfoldersByHash,
            shouldRenameByHash,
            formatErrorSubfolder,
            hashErrorSubfolder,
            withoutHashErrorSubfolder);
    }

    public void DeleteDestinationFolder()
    {
        DestinationFolder = DestinationFolder.Default;
    }

    public void AddSourceFolder(
        string path,
        bool shouldCheckFormat,
        bool shouldCheckHashFromName,
        bool shouldCreateTagsFromSubfolders,
        bool shouldAddTagFromFilename,
        IReadOnlyCollection<string> supportedExtensions)
    {
        var sourceFolder = new SourceFolder(
            Guid.NewGuid(),
            path,
            shouldCheckFormat,
            shouldCheckHashFromName,
            shouldCreateTagsFromSubfolders,
            shouldAddTagFromFilename,
            supportedExtensions);
        
        SourceFolders = SourceFolders.Append(sourceFolder).ToList();
    }

    public void RemoveSourceFolder(Guid id)
    {
        SourceFolders = SourceFolders.Where(x => x.Id != id).ToList();
    }

    public void UpdateSourceFolder(
        Guid sourceFolderId,
        string path,
        bool shouldCheckFormat,
        bool shouldCheckHashFromName,
        bool shouldCreateTagsFromSubfolders,
        bool shouldAddTagFromFilename,
        IReadOnlyCollection<string> supportedExtensions)
    {
        var sourceFolder = SourceFolders.FirstOrDefault(x => x.Id == sourceFolderId);
        if (sourceFolder == null)
            throw new ArgumentException($"Source folder with id {sourceFolderId} not found");

        sourceFolder = new SourceFolder(
            sourceFolderId,
            path,
            shouldCheckFormat,
            shouldCheckHashFromName,
            shouldCreateTagsFromSubfolders,
            shouldAddTagFromFilename,
            supportedExtensions);
        
        SourceFolders = SourceFolders.Where(x => x.Id != sourceFolderId).Append(sourceFolder).ToList();
    }
}
