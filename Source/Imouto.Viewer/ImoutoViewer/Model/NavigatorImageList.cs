using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Text.Json;
using ImoutoViewer.Extensions;

namespace ImoutoViewer.Model;

internal class NavigatorImageList(string selectedPath) : ILocalImageList
{
    private readonly CancellationTokenSource _lifetime = new();
    private bool _sortRequested;
    private List<string> _paths = [selectedPath];
    private int _index;

    public LocalImage CurrentImage { get; private set; } = new(selectedPath);

    public int ImagesCount => _paths.Count;

    public int? CurrentImageIndex => _index;

    public DirectoryInfo? CurrentDirectory { get; } = new FileInfo(selectedPath).Directory;

    public int DirectoriesCount => 1;

    public int? CurrentDirectoryIndex => 0;

    public bool IsEmpty => false;

    public bool IsDirectoryActive => false;

    public event EventHandler? CurrentImageChanged;

    public async Task LoadPlaylist(Guid playlistId, Func<CancellationToken, Task<IReadOnlyCollection<string>>> search)
    {
        var ct = _lifetime.Token;

        try
        {
            using var playlistTimeout = CancellationTokenSource.CreateLinkedTokenSource(ct);
            playlistTimeout.CancelAfter(TimeSpan.FromSeconds(30));

            using var pipe = new NamedPipeClientStream(
                ".",
                $"ImoutoViewer-{playlistId:N}",
                PipeDirection.In,
                PipeOptions.Asynchronous | PipeOptions.CurrentUserOnly);

            await pipe.ConnectAsync(playlistTimeout.Token);
            var paths = await JsonSerializer.DeserializeAsync<List<string>>(pipe, cancellationToken: playlistTimeout.Token);

            ct.ThrowIfCancellationRequested();

            if (paths != null && !paths.Any(string.IsNullOrWhiteSpace) && TrySetPlaylist(paths))
                return;
        }
        catch (Exception e) when (e is IOException or JsonException or UnauthorizedAccessException or OperationCanceledException)
        {
            Debug.WriteLine(e);
        }

        ct.ThrowIfCancellationRequested();

        var matches = await Task.Run(
            async () =>
            {
                var paths = await search(ct).ConfigureAwait(false);
                return paths.Where(path => path.IsImage()).ToList();
            },
            ct);

        ct.ThrowIfCancellationRequested();

        if (!TrySetPlaylist(matches) && matches.Count != 0)
        {
            matches.Insert(0, selectedPath);
            TrySetPlaylist(matches);
        }
    }

    private bool TrySetPlaylist(List<string> paths)
    {
        var index = paths.FindIndex(p => string.Equals(p, selectedPath, StringComparison.OrdinalIgnoreCase));
        if (index < 0)
            return false;

        _paths = paths;
        _index = index;

        if (_sortRequested)
            ResortFiles();

        return true;
    }

    public void Next() => MoveTo((_index + 1) % ImagesCount);

    public void Previous() => MoveTo((_index + ImagesCount - 1) % ImagesCount);

    public void NextFolder() => MoveTo(0);

    public void PrevFolder() => MoveTo(ImagesCount - 1);

    private void MoveTo(int index)
    {
        if (_index == index)
            return;

        CurrentImage.FreeMemory();
        _index = index;
        CurrentImage = new LocalImage(_paths[index]);

        CurrentImageChanged?.Invoke(this, EventArgs.Empty);
    }

    public void ResortFiles()
    {
        _sortRequested = true;
        _paths = LocalImageList.SortFiles(_paths, path => path).ToList();
        _index = _paths.FindIndex(p => string.Equals(p, CurrentImage.Path, StringComparison.OrdinalIgnoreCase));
    }

    public void Dispose()
    {
        _lifetime.Cancel();
        _lifetime.Dispose();
        CurrentImage.FreeMemory();
    }
}
