namespace ImoutoRebirth.Room.DataAccess.Models;

public class DestinationFolderCreateData
{
    public Guid CollectionId { get; }

    public string Path { get; }

    public bool ShouldCreateSubfoldersByHash { get; }

    public bool ShouldRenameByHash { get; }

    public string FormatErrorSubfolder { get; }

    public string HashErrorSubfolder { get; }

    public string WithoutHashErrorSubfolder { get; }

    public DestinationFolderCreateData(
        Guid collectionId,
        string path,
        bool shouldCreateSubfoldersByHash,
        bool shouldRenameByHash,
        string formatErrorSubfolder,
        string hashErrorSubfolder,
        string withoutHashErrorSubfolder)
    {
        FormatErrorSubfolder = formatErrorSubfolder 
                               ?? throw new ArgumentNullException(nameof(formatErrorSubfolder));
        HashErrorSubfolder = hashErrorSubfolder 
                             ?? throw new ArgumentNullException(nameof(hashErrorSubfolder));
        WithoutHashErrorSubfolder = withoutHashErrorSubfolder 
                                    ?? throw new ArgumentNullException(nameof(withoutHashErrorSubfolder));
        Path = path
               ?? throw new ArgumentNullException(nameof(path));

        CollectionId = collectionId;
        ShouldCreateSubfoldersByHash = shouldCreateSubfoldersByHash;
        ShouldRenameByHash = shouldRenameByHash;
    }
}