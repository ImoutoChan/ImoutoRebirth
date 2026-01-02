namespace ImoutoRebirth.Room.Domain.CollectionAggregate;

public class DestinationFolder
{
    public DestinationFolder(
        Guid id,
        string? path,
        bool shouldCreateSubfoldersByHash,
        bool shouldRenameByHash,
        string formatErrorSubfolder,
        string hashErrorSubfolder,
        string withoutHashErrorSubfolder)
    {
        Id = id;
        ShouldCreateSubfoldersByHash = shouldCreateSubfoldersByHash;
        ShouldRenameByHash = shouldRenameByHash;
        FormatErrorSubfolder = formatErrorSubfolder;
        HashErrorSubfolder = hashErrorSubfolder;
        WithoutHashErrorSubfolder = withoutHashErrorSubfolder;
        DestinationDirectory = string.IsNullOrWhiteSpace(path) ? null : new DirectoryInfo(path);
    }

    public Guid Id { get; }

    public bool ShouldCreateSubfoldersByHash { get; }

    public bool ShouldRenameByHash { get; }

    public string FormatErrorSubfolder { get; }

    public string HashErrorSubfolder { get; }

    public string WithoutHashErrorSubfolder { get; }

    public DirectoryInfo? DestinationDirectory { get; }

    public static DestinationFolder Default
        => new(
            Guid.Empty,
            null,
            false, 
            false, 
            DefaultValues.DestinationFolderEntityFormatErrorSubfolder,
            DefaultValues.DestinationFolderEntityHashErrorSubfolder,
            DefaultValues.DestinationFolderEntityWithoutHashErrorSubfolder);
    
    public bool IsDefault() => Id == Guid.Empty;

    public bool AllowManualFileRenaming() => !ShouldRenameByHash;

    public SystemFileMoved Move(SystemFilePreparedToMove preparedToMove)
    {
        if (preparedToMove.MoveProblem == MoveProblem.AlreadyContains)
        {
            var shouldDelete = DestinationDirectory != null;

            if (shouldDelete)
                preparedToMove.SystemFile.File.Delete();

            return preparedToMove.CreateMovedFail(
                $"File with this md5 already contains in database | " +
                $"Md5: {preparedToMove.SystemFile.Md5} " +
                $"File: {preparedToMove.SystemFile.File.FullName} | " +
                $"File deleted: {shouldDelete}");
        }

        var newFile = GetNewPath(preparedToMove);
        
        var wasANewFile = preparedToMove.SystemFile.MoveFile(ref newFile);

        return preparedToMove.CreateMoved(wasANewFile && preparedToMove.MoveProblem == MoveProblem.None, newFile);
    }

    private FileInfo GetNewPath(SystemFilePreparedToMove moveInformation)
    {
        var newPathParts = new List<string>();

        AddDestinationFolder(moveInformation, newPathParts);

        var problemSubfolder = GetProblemSubfolder(moveInformation.MoveProblem);

        var renamed = false;

        if (problemSubfolder != null)
        {
            newPathParts.Add(problemSubfolder);
        }
        else
        {
            if (ShouldCreateSubfoldersByHash) 
                AddMd5Path(moveInformation, newPathParts);

            if (ShouldRenameByHash)
            {
                RenameToMd5(moveInformation, newPathParts);
                renamed = true;
            }
        }

        if (!renamed)
            newPathParts.Add(moveInformation.SystemFile.File.Name);

        return new FileInfo(Path.Combine(newPathParts.ToArray()));
    }

    private static void RenameToMd5(SystemFilePreparedToMove moveInformation, List<string> newPathParts)
    {
        newPathParts.Add($"{moveInformation.SystemFile.Md5}{moveInformation.SystemFile.File.Extension}");
    }

    private static void AddMd5Path(SystemFilePreparedToMove moveInformation, List<string> newPathParts)
    {
        var md5Sub = moveInformation.SystemFile.Md5.Substring(0, 2);
        var md5SubSub = moveInformation.SystemFile.Md5.Substring(2, 2);

        newPathParts.Add(md5Sub);
        newPathParts.Add(md5SubSub);
    }

    private void AddDestinationFolder(
        SystemFilePreparedToMove moveInformation,
        ICollection<string> newPathParts)
    {
        var destDirectory = DestinationDirectory;
        var fileDirectory = moveInformation.SystemFile.File.DirectoryName;

        if (destDirectory == null)
        {
            if (fileDirectory != null)
                newPathParts.Add(fileDirectory);
        }
        else
        {
            newPathParts.Add(destDirectory.FullName);
        }
    }

    private string? GetProblemSubfolder(MoveProblem moveProblem) => moveProblem switch
    {
        MoveProblem.None => null,
        MoveProblem.InvalidFormat => FormatErrorSubfolder,
        MoveProblem.WithoutHash => WithoutHashErrorSubfolder,
        MoveProblem.IncorrectHash => HashErrorSubfolder,
        _ => throw new ArgumentOutOfRangeException(nameof(moveProblem), moveProblem, null)
    };
}

file static class DefaultValues
{
    public static string DestinationFolderEntityFormatErrorSubfolder => "!FormatError";
    public static string DestinationFolderEntityHashErrorSubfolder => "!HashError";
    public static string DestinationFolderEntityWithoutHashErrorSubfolder => "!WithoutHashError";
}
