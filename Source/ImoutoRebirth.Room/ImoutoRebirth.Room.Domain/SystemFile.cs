using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Security.AccessControl;
using System.Security.Cryptography;
using Microsoft.Win32.SafeHandles;
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

    public IReadOnlyCollection<string> GetTagsFromName() => [File.Name];

    public IReadOnlyCollection<string> GetTagsFromPathForSourceFolder(SourceFolder sourceFolder) 
        => GetTags(sourceFolder).ToList();

    public void MoveFile(ref FileInfo newFile)
    {
        if (string.Equals(File.FullName, newFile.FullName, StringComparison.OrdinalIgnoreCase))
            return;

        if (!File.Exists)
            throw new ArgumentException(nameof(File));

        if (newFile.Directory is { Exists: false }) 
            newFile.Directory.Create();

        if (newFile.Exists)
        {
            MoveToExisted(ref newFile);
        }
        else
        {
            System.IO.File.Move(File.FullName, newFile.FullName);
        }
    }

    public void Delete() => File.Delete();

    public bool DeleteIfDuplicateOf(FileInfo retainedFile)
    {
        if (!OperatingSystem.IsWindows()
            || string.Equals(File.FullName, retainedFile.FullName, StringComparison.OrdinalIgnoreCase)
            || !retainedFile.Exists)
            return false;

        using var retained = retainedFile.Open(FileMode.Open, FileAccess.Read, FileShare.Read);
        using var incoming = File.Create(
            FileMode.Open,
            FileSystemRights.Read | FileSystemRights.Delete,
            FileShare.Read,
            4096,
            FileOptions.SequentialScan,
            null);

        if (retained.Length != incoming.Length)
            return false;

        var incomingBytes = new byte[64 * 1024];
        var retainedBytes = new byte[incomingBytes.Length];
        int read;
        while ((read = incoming.Read(incomingBytes)) > 0)
        {
            retained.ReadExactly(retainedBytes.AsSpan(0, read));
            if (!incomingBytes.AsSpan(0, read).SequenceEqual(retainedBytes.AsSpan(0, read)))
                return false;
        }

        const int fileDispositionInfo = 4;
        byte deleteOnClose = 1;
        if (!SetFileInformationByHandle(incoming.SafeFileHandle, fileDispositionInfo, ref deleteOnClose, sizeof(byte)))
            throw new IOException("Unable to delete the verified duplicate.", new Win32Exception(Marshal.GetLastWin32Error()));

        return true;
    }

    [DllImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool SetFileInformationByHandle(
        SafeFileHandle file, int infoClass, ref byte value, int size);

    private void MoveToExisted(ref FileInfo newFile)
    {
        if (DeleteIfDuplicateOf(newFile))
            return;

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
    }

    private static bool IsFileReady(FileInfo fileInfo)
    {
        // If the file can be opened for exclusive access, it means that the file
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

