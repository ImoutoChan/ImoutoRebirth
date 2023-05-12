using ImoutoRebirth.Room.DataAccess.Models.Abstract;

namespace ImoutoRebirth.Room.DataAccess.Models;

public abstract class DestinationFolder : ModelBase
{
    public Guid CollectionId { get; }

    public bool ShouldCreateSubfoldersByHash { get; }

    public bool ShouldRenameByHash { get; }

    public string FormatErrorSubfolder { get; }

    public string HashErrorSubfolder { get; }

    public string WithoutHashErrorSubfolder { get; }

    protected DestinationFolder(
        Guid id,
        Guid collectionId,
        bool shouldCreateSubfoldersByHash,
        bool shouldRenameByHash,
        string formatErrorSubfolder,
        string hashErrorSubfolder,
        string withoutHashErrorSubfolder) 
        : base(id)
    {
        CollectionId = collectionId;
        ShouldCreateSubfoldersByHash = shouldCreateSubfoldersByHash;
        ShouldRenameByHash = shouldRenameByHash;
        FormatErrorSubfolder = formatErrorSubfolder;
        HashErrorSubfolder = hashErrorSubfolder;
        WithoutHashErrorSubfolder = withoutHashErrorSubfolder;
    }

    public abstract DirectoryInfo? GetDestinationDirectory();
}