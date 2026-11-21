using System.IO;
using System.IO.Compression;
using ImoutoViewer.Extensions;
using SharpCompress.Archives;
using SharpCompress.Common;

namespace ImoutoViewer.Model.ArchiveSupport;

internal class ArchiveImageList : ILocalImageList
{
    private readonly ZipArchive? _zip;
    private readonly Dictionary<LocalImage, ZipArchiveEntry> _pendingPages = new();
    private readonly TemporaryDirectoryManager _tempDirManager;
    private readonly IReadOnlyList<LocalImage> _images;
    private int _currentIndex;
    private int _requestedIndex;
    private CancellationTokenSource? _extraction;
    private Task? _readTask;
    private bool _disposed;

    private ArchiveImageList(string archiveFilePath, IProgress<ProgressReport> progress)
    {
        var archive = new FileInfo(archiveFilePath);

        if (!IsSupportedArchive(archive))
            throw new ArgumentException("Unsupported archive type", nameof(archiveFilePath));

        _tempDirManager = new TemporaryDirectoryManager();

        try
        {
            if (IsZip(archive))
            {
                _zip = ZipFile.OpenRead(archive.FullName);
                _images = IndexZipPages(_zip);
            }
            else
            {
                using var otherArchive = ArchiveFactory.OpenArchive(archive.FullName);
                otherArchive.WriteToDirectory(
                    _tempDirManager.TempDirectoryPath,
                    progress: progress);

                _images = Directory.EnumerateFiles(_tempDirManager.TempDirectoryPath, "*", SearchOption.AllDirectories)
                    .Where(path => path.IsImage())
                    .OrderBy(path => path.GetAlphanumericName(), StringComparer.OrdinalIgnoreCase)
                    .Select(path => new LocalImage(path))
                    .ToList();
            }

            _requestedIndex = _currentIndex = _images.Count == 0 ? -1 : 0;
        }
        catch
        {
            _zip?.Dispose();
            _tempDirManager.Dispose();
            throw;
        }
    }

    public static async Task<ArchiveImageList> OpenAsync(string path, IProgress<ProgressReport> progress, CancellationToken ct)
    {
        var list = new ArchiveImageList(path, progress);

        try
        {
            if (list.CurrentImage is { } first)
            {
                list._pendingPages.Remove(first, out var entry);
                await PreparePage(first, entry, ct).ConfigureAwait(false);
            }

            ct.ThrowIfCancellationRequested();

            return list;
        }
        catch
        {
            list.Dispose();
            throw;
        }
    }

    public static bool IsSupportedArchive(string archiveFile) => IsSupportedArchive(new FileInfo(archiveFile));

    public static bool IsSupportedArchive(FileInfo archiveFile)
    {
        if (archiveFile.Extension is "" or null)
            return false;

        return IsZip(archiveFile) || CanDetectArchiveFormat(archiveFile);
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

    public void Next() => Move(1);

    public void Previous() => Move(-1);

    private async void Move(int offset)
    {
        if (_disposed || _images.Count == 0)
            return;

        _requestedIndex = (_requestedIndex + offset + _images.Count) % _images.Count;
        if (_extraction != null)
        {
            _extraction.Cancel();
            return;
        }

        while (_requestedIndex != _currentIndex)
        {
            var index = _requestedIndex;
            var page = _images[index];
            _pendingPages.TryGetValue(page, out var entry);
            using var request = new CancellationTokenSource();
            _extraction = request;

            try
            {
                _readTask = Task.Run(() => PreparePage(page, entry, request.Token), request.Token);
                await _readTask;
                _pendingPages.Remove(page);
            }
            catch (OperationCanceledException) when (request.IsCancellationRequested)
            {
            }
            finally
            {
                _extraction = null;
                _readTask = null;
            }

            if (_disposed || request.IsCancellationRequested)
            {
                page.FreeMemory();
                if (_disposed)
                    return;
                continue;
            }

            CurrentImage?.FreeMemory();
            _currentIndex = index;
            CurrentImageChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    public void NextFolder() {}

    public void PrevFolder() {}

    public void ResortFiles()
    {
    }

    private IReadOnlyList<LocalImage> IndexZipPages(ZipArchive zip)
    {
        var entries = zip.Entries
            .Where(entry => entry.FullName.IsImage())
            .OrderBy(entry => entry.FullName.Replace('/', '\\').GetAlphanumericName(), StringComparer.OrdinalIgnoreCase);

        var pages = new List<LocalImage>();
        foreach (var entry in entries)
        {
            var fileName = Path.GetFileName(entry.FullName);
            if (fileName.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
                continue;

            var pageDirectory = Path.Combine(_tempDirManager.TempDirectoryPath, pages.Count.ToString());
            var page = new LocalImage(Path.Combine(pageDirectory, fileName));

            _pendingPages.Add(page, entry);
            pages.Add(page);
        }

        return pages;
    }

    private static async Task PreparePage(LocalImage page, ZipArchiveEntry? entry, CancellationToken ct)
    {
        if (entry != null)
            await ExtractPage(page, entry, ct).ConfigureAwait(false);

        ct.ThrowIfCancellationRequested();
        page.Preload();
    }

    private static async Task ExtractPage(LocalImage page, ZipArchiveEntry entry, CancellationToken ct)
    {
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(page.Path)!);
            using var input = entry.Open();
            using var output = new FileStream(page.Path, FileMode.Create, FileAccess.Write, FileShare.None, 81920, true);
            await input.CopyToAsync(output, ct).ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
            File.Delete(page.Path);
            throw;
        }
        catch (Exception error) when (error is IOException or InvalidDataException or UnauthorizedAccessException)
        {
            page.SetError(error);
        }
    }

    private static bool IsZip(FileInfo file)
        => file.Extension.Equals(".zip", StringComparison.OrdinalIgnoreCase)
           || file.Extension.Equals(".cbz", StringComparison.OrdinalIgnoreCase);

    private static bool CanDetectArchiveFormat(FileInfo archiveFile)
    {
        try
        {
            return ArchiveFactory.IsArchive(archiveFile.FullName, out _);
        }
        catch
        {
            return false;
        }
    }

    public void Dispose()
    {
        if (_disposed)
            return;

        _disposed = true;
        CurrentImage?.FreeMemory();
        _extraction?.Cancel();
        if (_readTask is { IsCompleted: false } reading)
        {
            _ = reading.ContinueWith(_ => CloseArchive(), TaskScheduler.Default);
        }
        else
        {
            CloseArchive();
        }
    }

    private void CloseArchive()
    {
        _zip?.Dispose();
        _tempDirManager.Dispose();
    }
}
