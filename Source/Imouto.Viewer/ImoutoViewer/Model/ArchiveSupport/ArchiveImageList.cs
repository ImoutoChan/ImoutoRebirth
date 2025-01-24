using System.IO;
using System.IO.Compression;
using ImoutoViewer.Extensions;
using SharpCompress.Archives;

namespace ImoutoViewer.Model.ArchiveSupport;

internal class ArchiveImageList : ILocalImageList
{
    private readonly Action<double> _loadingProgressReportAction;
    private readonly TemporaryDirectoryManager _tempDirManager;
    private readonly List<LocalImage> _images;
    private int _currentIndex;

    public ArchiveImageList(string archiveFilePath, Action<double> loadingProgressReportAction)
    {
        var archive = new FileInfo(archiveFilePath);

        _loadingProgressReportAction = loadingProgressReportAction;

        if (!IsSupportedArchive(archive))
            throw new ArgumentException("Unsupported archive type", nameof(archiveFilePath));

        _tempDirManager = new TemporaryDirectoryManager();
        _images = LoadImagesFromArchive(archive);
        ResortFiles();
        _currentIndex = _images.Any() ? 0 : -1;
    }

    public static bool IsSupportedArchive(string archiveFile) => IsSupportedArchive(new FileInfo(archiveFile));

    public static bool IsSupportedArchive(FileInfo archiveFile)
    {
        if (archiveFile.Extension is "" or null)
            return false;

        return archiveFile.Extension switch
        {
            ".zip" => true,
            ".cbz" => true,
            _ => false
        } || ArchiveFactory.IsArchive(archiveFile.FullName, out _);
    }

    public LocalImage? CurrentImage => _currentIndex >= 0 && _currentIndex < _images.Count
        ? _images[_currentIndex]
        : null;

    public int ImagesCount => _images.Count;

    public int? CurrentImageIndex => _currentIndex >= 0 ? _currentIndex : (int?)null;

    public DirectoryInfo? CurrentDirectory => null;

    public int DirectoriesCount => 0;

    public int? CurrentDirectoryIndex => null;

    public bool IsEmpty => _images.Count == 0;

    public bool IsDirectoryActive => false;

    public event EventHandler? CurrentImageChanged;

    public void Next()
    {
        if (_images.Count > 0)
        {
            _currentIndex = (_currentIndex + 1) % _images.Count;
            CurrentImageChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    public void Previous()
    {
        if (_images.Count > 0)
        {
            _currentIndex = (_currentIndex - 1 + _images.Count) % _images.Count;
            CurrentImageChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    public void NextFolder() {}

    public void PrevFolder() {}

    public void ResortFiles()
    {
        _images.Sort((img1, img2) => string.Compare(img1.Path, img2.Path, StringComparison.OrdinalIgnoreCase));
    }

    private List<LocalImage> LoadImagesFromArchive(FileInfo archiveFile)
    {
        try
        {
            var imagePaths = ExtractImagePaths(archiveFile);
            return imagePaths.Select(path => new LocalImage(path)).ToList();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    private List<string> ExtractImagePaths(FileInfo archiveFile)
    {
        var extension = archiveFile.Extension.ToLower();
        var imagePaths = new List<string>();

        if (extension is ".zip" or ".cbz")
        {
            ZipFile.ExtractToDirectory(archiveFile.FullName, _tempDirManager.TempDirectoryPath);
        }
        else // if (extension is ".rar" or ".7z" or ...)
        {
            using var archive = ArchiveFactory.Open(archiveFile.FullName);
            archive.ExtractToDirectory(_tempDirManager.TempDirectoryPath, _loadingProgressReportAction);
        }

        var extractedFiles = Directory.GetFiles(_tempDirManager.TempDirectoryPath, "*.*", SearchOption.AllDirectories);
        imagePaths.AddRange(extractedFiles.Where(file => file.IsImage()));

        return imagePaths;
    }

    public void Dispose() => _tempDirManager.Dispose();
}
