using System.Security.Cryptography;
using System.Text.RegularExpressions;
using ImoutoRebirth.Room.Domain.CollectionAggregate;

namespace ImoutoRebirth.Room.Domain;

public partial class SystemFile
{
    public FileInfo File { get; }

    public string Md5 { get; }

    public long Size { get; }

    public SystemFile(FileInfo file, string md5, long size)
    {
        File = file;
        Md5 = md5;
        Size = size;
    }

    public static SystemFile? Create(FileInfo file)
    {
        if (!IsFileReady(file))
            return null;
        
        using var md5 = MD5.Create();
        using var stream = file.OpenRead();
        var computed = md5.ComputeHash(stream);
        
        var md5Hash = BitConverter.ToString(computed).Replace("-", "").ToLower();
        return new SystemFile(file, md5Hash, file.Length);
    }

    public bool IsFileReady() => IsFileReady(File);

    public bool TryGetHashFromFileName(out string? hash)
    {
        hash = null;
        var match = Md5HashRegex().Match(File.Name);

        if (!match.Success)
            return false;

        hash = match.Value;
        return true;
    }

    public IReadOnlyCollection<string> GetTagsFromName() => new[] {File.Name};

    public IReadOnlyCollection<string> GetTagsFromPathForSourceFolder(SourceFolder sourceFolder) 
        => GetTags(sourceFolder).ToList();

    public bool MoveFile(ref FileInfo newFile)
    {
        if (string.Equals(File.FullName, newFile.FullName, StringComparison.OrdinalIgnoreCase))
            return true;

        if (!File.Exists)
            throw new ArgumentException(nameof(File));

        if (newFile.Directory is { Exists: false }) 
            newFile.Directory.Create();

        if (newFile.Exists)
            return MoveToExisted(ref newFile);

        System.IO.File.Move(File.FullName, newFile.FullName);

        return true;
    }

    public void Delete() => File.Delete();

    private bool MoveToExisted(ref FileInfo newFile)
    {
        var newSystemFile = Create(newFile);
        
        if (Md5 == newSystemFile!.Md5)
        {
            File.Delete();
            return false;
        }

        var counter = 0;
        FileInfo countedFile;

        var filenameWithoutExtension = newFile.Name[..^newFile.Extension.Length];

        do
        {
            var countedPath = $@"{newFile.Directory}\{filenameWithoutExtension} ({counter}){newFile.Extension}";
            countedFile = new FileInfo(countedPath);
            counter++;
        }
        while (countedFile.Exists);

        File.MoveTo(countedFile.FullName);
        newFile = countedFile;

        return true;
    }

    private static bool IsFileReady(FileInfo fileInfo)
    {
        // If the file can be opened for exclusive access it means that the file
        // is no longer locked by another process.
        try
        {
            using (fileInfo.Open(FileMode.Open, FileAccess.Read, FileShare.None))
                return true;
        }
        catch (Exception)
        {
            return false;
        }
    }

    private IEnumerable<string> GetTags(SourceFolder sourceFolder)
    {
        var sourcePathEntries = GetPathParts(new DirectoryInfo(sourceFolder.Path));
        var filePathEntries = GetPathParts(File);

        return filePathEntries.Except(sourcePathEntries);
    }

    private static IEnumerable<string> GetPathParts(DirectoryInfo directoryInfo)
    {
        var directory = directoryInfo;

        while(directory != null)
        {
            yield return directory.Name;
            directory = directory.Parent;
        }
    }

    private static IEnumerable<string> GetPathParts(FileInfo fileInfo)
    {
        if (fileInfo.Directory == null)
            yield break;
            
        foreach (var directoryPart in GetPathParts(fileInfo.Directory))
        {
            yield return directoryPart;
        }
    }

    [GeneratedRegex(
        "[0-9a-f]{32}",
        RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.NonBacktracking | RegexOptions.CultureInvariant)]
    private static partial Regex Md5HashRegex();
}

