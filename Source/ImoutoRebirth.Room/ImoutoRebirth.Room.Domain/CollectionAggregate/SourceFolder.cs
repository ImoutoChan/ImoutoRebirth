using ImoutoRebirth.Common;

namespace ImoutoRebirth.Room.Domain.CollectionAggregate;

public class SourceFolder
{
    public SourceFolder(
        Guid id,
        string path,
        bool shouldCheckFormat,
        bool shouldCheckHashFromName,
        bool shouldCreateTagsFromSubfolders,
        bool shouldAddTagFromFilename,
        IReadOnlyCollection<string> supportedExtensions,
        bool isWebhookUploadEnabled,
        string? webhookUploadUrl)
    {
        Id = id;
        Path = path;
        ShouldCheckFormat = shouldCheckFormat;
        ShouldCheckHashFromName = shouldCheckHashFromName;
        ShouldCreateTagsFromSubfolders = shouldCreateTagsFromSubfolders;
        ShouldAddTagFromFilename = shouldAddTagFromFilename;
        SupportedExtensions = supportedExtensions;
        IsWebhookUploadEnabled = isWebhookUploadEnabled;
        WebhookUploadUrl = webhookUploadUrl;

        if (isWebhookUploadEnabled)
            ArgumentValidator.NotNullOrWhiteSpace(() => webhookUploadUrl);
    }

    public Guid Id { get; }

    public string Path { get; }

    public bool ShouldCheckFormat { get; }

    public bool ShouldCheckHashFromName { get; }

    public bool ShouldCreateTagsFromSubfolders { get; }

    public bool ShouldAddTagFromFilename { get; }

    public IReadOnlyCollection<string> SupportedExtensions { get; }

    public bool IsWebhookUploadEnabled { get; }

    public string? WebhookUploadUrl { get; }

    public IEnumerable<FileInfo> GetAllFileInfo(DestinationFolder? destination = null)
    {
        var root = new DirectoryInfo(Path);
        if (!root.Exists)
            return [];

        destination ??= DestinationFolder.Default;
        var quarantine = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            destination.FormatErrorSubfolder,
            destination.HashErrorSubfolder,
            destination.WithoutHashErrorSubfolder
        };

        var files = EnumerateSourceDirectories(root).SelectMany(directory =>
            SupportedExtensions.SafeNone()
                ? directory.EnumerateFiles()
                : SupportedExtensions.SelectMany(ext => directory.EnumerateFiles("*." + ext)));

        return files.OrderBy(file => file.LastWriteTimeUtc).DistinctBy(file => file.FullName);

        IEnumerable<DirectoryInfo> EnumerateSourceDirectories(DirectoryInfo directory)
        {
            yield return directory;
            foreach (var child in directory.EnumerateDirectories())
            {
                if (destination.DestinationDirectory == null && quarantine.Contains(child.Name))
                    continue;
                foreach (var descendant in EnumerateSourceDirectories(child))
                    yield return descendant;
            }
        }
    }
    
    public SystemFilePreparedToMove PrepareFileToMove(
        SystemFile file, 
        string? existingFilePath,
        bool isCorrectImage)
    {
        var moveInformation = new SystemFilePreparedToMove(file, existingFilePath);

        moveInformation.SetProblem(FindProblems(file, existingFilePath != null, isCorrectImage));

        if (ShouldAddTagFromFilename) 
            moveInformation.AddSourceTags(file.GetTagsFromName());

        if (ShouldCreateTagsFromSubfolders) 
            moveInformation.AddSourceTags(file.GetTagsFromPathForSourceFolder(this));

        return moveInformation;
    }
    
    private MoveProblem FindProblems(
        SystemFile systemFile,
        bool hasFileInCollectionWithSameMd5,
        bool isCorrectImage)
    {
        if (hasFileInCollectionWithSameMd5)
            return MoveProblem.AlreadyContains;

        if (ShouldCheckFormat && !isCorrectImage)
            return MoveProblem.InvalidFormat;

        if (ShouldCheckHashFromName)
        {
            if (!systemFile.TryGetHashFromFileName(out var nameHash))
                return MoveProblem.WithoutHash;

            if (!string.Equals(nameHash, systemFile.Md5, StringComparison.OrdinalIgnoreCase))
                return MoveProblem.IncorrectHash;
        }

        return MoveProblem.None;
    }
}
