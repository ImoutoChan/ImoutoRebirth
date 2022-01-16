using System.Security.Cryptography;
using ImoutoRebirth.Room.Core.Models;
using ImoutoRebirth.Room.Core.Services.Abstract;
using Microsoft.Extensions.Logging;

namespace ImoutoRebirth.Room.Infrastructure.Service;

public class FileService : IFileService
{
    private readonly ILogger<FileService> _logger;

    public FileService(ILogger<FileService> logger)
    {
        _logger = logger;
    }

    public IReadOnlyCollection<FileInfo> GetFiles(
        DirectoryInfo directoryInfo,
        IReadOnlyCollection<string>? supportedExtensions,
        SearchOption searchOption = SearchOption.AllDirectories)
    {
        return supportedExtensions?.Any() != true
            ? directoryInfo.GetFiles("*.*", searchOption)
            : supportedExtensions.Select(x => "*." + x)
                .SelectMany(x => directoryInfo.EnumerateFiles(x, searchOption))
                .ToArray();
    }

    /// <summary>
    /// Check availability of file by trying to seizing exclusive access.
    /// </summary>
    /// <param name="fileInfo">File to check availability.</param>
    /// <returns>Indicates whenever file is available for exclusive access.</returns>
    public bool IsFileReady(FileInfo fileInfo)
    {
        // If the file can be opened for exclusive access it means that the file
        // is no longer locked by another process.
        try
        {
            using (fileInfo.Open(FileMode.Open, FileAccess.Read, FileShare.None))
            {
                return true;
            }
        }
        catch (Exception)
        {
            return false;
        }
    }

    public string GetMd5Checksum(FileInfo fileInfo)
    {
        using var md5 = MD5.Create();
        using var stream = fileInfo.OpenRead();

        return BitConverter.ToString(md5.ComputeHash(stream)).Replace("-", "").ToLower();
    }

    /// <summary>
    /// Move file from current to desired location.
    /// </summary>
    /// <param name="oldFile">Current file location.</param>
    /// <param name="newFile">Desired file location.</param>
    /// <returns>Should file be registered.</returns>
    public bool MoveFile(SystemFile oldFile, ref FileInfo newFile)
    {
        if (string.Equals(oldFile.File.FullName, newFile.FullName, StringComparison.OrdinalIgnoreCase))
        {
            _logger.LogInformation("File doesn't require moving.");
            return true;
        }

        if (!oldFile.File.Exists)
            throw new ArgumentException(nameof(oldFile));

        if (newFile.Directory != null && !newFile.Directory.Exists)
        {
            _logger.LogInformation("Creating target directory {NewDirectory}", newFile.Directory);
            newFile.Directory.Create();
        }

        if (newFile.Exists)
            return MoveToExisted(oldFile, ref newFile);

        _logger.LogInformation("Moving to {NewPath}.", newFile.FullName);
        oldFile.File.MoveTo(newFile.FullName);

        return true;
    }

    private bool MoveToExisted(SystemFile oldFile, ref FileInfo newFile)
    {
        _logger.LogWarning("Destination file already exists.");

        if (oldFile.Md5 == GetMd5Checksum(newFile))
        {
            _logger.LogInformation("Files are identical. Removing file from source folder.");

            oldFile.File.Delete();

            return false;
        }

        _logger.LogInformation("Files are different. Generating new name.");

        int counter = 0;
        FileInfo countedFile;

        var filenameWithoutExtension =
            newFile.Name.Substring(0, newFile.Name.Length - newFile.Extension.Length - 1);

        do
        {
            var countedPath
                = $@"{newFile.Directory}\{filenameWithoutExtension} ({counter}).{newFile.Extension}";
            countedFile = new FileInfo(countedPath);
            counter++;
        }
        while (countedFile.Exists);

        _logger.LogInformation("Moving to {ChangeNewPath}.", countedFile.FullName);
        oldFile.File.MoveTo(countedFile.FullName);

        newFile = countedFile;

        return true;
    }
}