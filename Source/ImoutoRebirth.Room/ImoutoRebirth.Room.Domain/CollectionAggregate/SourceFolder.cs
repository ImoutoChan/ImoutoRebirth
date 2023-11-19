﻿using ImoutoRebirth.Common;

namespace ImoutoRebirth.Room.Domain;

public class SourceFolder
{
    public SourceFolder(
        Guid id,
        Guid collectionId,
        string path,
        bool shouldCheckFormat,
        bool shouldCheckHashFromName,
        bool shouldCreateTagsFromSubfolders,
        bool shouldAddTagFromFilename,
        IReadOnlyCollection<string> supportedExtensions)
    {
        Id = id;
        CollectionId = collectionId;
        Path = path;
        ShouldCheckFormat = shouldCheckFormat;
        ShouldCheckHashFromName = shouldCheckHashFromName;
        ShouldCreateTagsFromSubfolders = shouldCreateTagsFromSubfolders;
        ShouldAddTagFromFilename = shouldAddTagFromFilename;
        SupportedExtensions = supportedExtensions;
    }

    public Guid Id { get; }

    public Guid CollectionId { get; }

    public string Path { get; }

    public bool ShouldCheckFormat { get; }

    public bool ShouldCheckHashFromName { get; }

    public bool ShouldCreateTagsFromSubfolders { get; }

    public bool ShouldAddTagFromFilename { get; }

    public IReadOnlyCollection<string> SupportedExtensions { get; }

    public IEnumerable<FileInfo> GetAllFileInfo()
    {
        var directoryInfo = new DirectoryInfo(Path);

        if (!directoryInfo.Exists)
            return Enumerable.Empty<FileInfo>();

        return 
            SupportedExtensions.SafeNone()
                ? directoryInfo.GetFiles("*.*", SearchOption.AllDirectories)
                : SupportedExtensions.Select(x => "*." + x)
                    .SelectMany(x => directoryInfo.EnumerateFiles(x, SearchOption.AllDirectories))
                    .OrderBy(x => x.LastWriteTimeUtc)
                    .DistinctBy(x => x.FullName);
    }
    
    public SystemFilePreparedToMove PrepareFileToMove(
        SystemFile file, 
        bool hasFileInCollectionWithSameMd5,
        bool isCorrectImage)
    {
        var moveInformation = new SystemFilePreparedToMove(file);

        moveInformation.SetProblem(FindProblems(file, hasFileInCollectionWithSameMd5, isCorrectImage));

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
