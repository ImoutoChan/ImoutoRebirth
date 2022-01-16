namespace ImoutoRebirth.Room.DataAccess.Models;

public class SourceFolderCreateData
{
    public Guid CollectionId { get; }

    public string Path { get; }

    public bool ShouldCheckFormat { get; }

    public bool ShouldCheckHashFromName { get; }

    public bool ShouldCreateTagsFromSubfolders { get; }

    public bool ShouldAddTagFromFilename { get; }

    public IReadOnlyCollection<string> SupportedExtensions { get; }

    public SourceFolderCreateData(
        Guid collectionId,
        string path,
        bool shouldCheckFormat,
        bool shouldCheckHashFromName,
        bool shouldCreateTagsFromSubfolders,
        bool shouldAddTagFromFilename,
        IReadOnlyCollection<string> supportedExtensions)
    {
        Path = path;
        ShouldCheckFormat = shouldCheckFormat;
        ShouldCheckHashFromName = shouldCheckHashFromName;
        ShouldCreateTagsFromSubfolders = shouldCreateTagsFromSubfolders;
        ShouldAddTagFromFilename = shouldAddTagFromFilename;
        SupportedExtensions = supportedExtensions;
        CollectionId = collectionId;
    }
}