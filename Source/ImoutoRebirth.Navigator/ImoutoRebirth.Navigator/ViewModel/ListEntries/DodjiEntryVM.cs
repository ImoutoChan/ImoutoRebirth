using System.IO;
using System.Windows;
using System.Windows.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using ImoutoRebirth.Common;
using ImoutoRebirth.Navigator.Model;
using Serilog;
using SharpCompress.Archives;
using SharpCompress.Common;

namespace ImoutoRebirth.Navigator.ViewModel.ListEntries;

internal partial class DodjiEntryVM : BaseEntryVM, INavigatorListEntry, IPixelSizable
{
    private bool _isLoaded;

    [ObservableProperty]
    private Size _viewPortSize;

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private BitmapSource? _image;

    public DodjiEntryVM(
        string path,
        Size viewPortSize,
        Guid? dbId)
        : base(dbId)
    {
        ViewPortSize = viewPortSize;
        Path = path;
    }

    public ListEntryType Type => ListEntryType.Dodji;

    public string Path { get; }

    public Size? PixelSize => Image != null ? new Size(Image.PixelWidth, Image.PixelHeight) : null;

    public object Data => new DataObject(DataFormats.FileDrop, new[] { Path });

    public DragDropEffects AllowDragDropEffects => DragDropEffects.Copy;

    public async void Load()
    {
        if (_isLoaded || IsLoading)
            return;

        IsLoading = true;

        try
        {
            Image = await ImageEntry.PreviewLoadingThreadQueue.Run(LoadCover);
            OnPropertyChanged(nameof(PixelSize));
            _isLoaded = true;
        }
        catch (OperationCanceledException)
        {
        }
        catch (Exception e) when (e is IOException or UnauthorizedAccessException or InvalidDataException
                                  or SharpCompressException or NotSupportedException or FormatException)
        {
            Log.Warning(e, "Unable to read archive cover {Path}", Path);
        }
        finally
        {
            IsLoading = false;
        }
    }

    private BitmapSource? LoadCover()
    {
        using var archive = ArchiveFactory.OpenArchive(Path);
        var preview = archive.Entries.FirstOrDefault(x => x is { Size: > 0, IsDirectory: false }
            && (x.Key.EndsWithIgnoreCase(".jpg") || x.Key.EndsWithIgnoreCase(".jpeg") || x.Key.EndsWithIgnoreCase(".png")))
            ?? archive.Entries.FirstOrDefault(x => x is { Size: > 0, IsDirectory: false });

        if (preview == null)
            return null;

        using var stream = preview.OpenEntryStream();
        using var buffer = new MemoryStream();

        stream.CopyTo(buffer);
        buffer.Position = 0;

        var image = BitmapFrame.Create(buffer, BitmapCreateOptions.None, BitmapCacheOption.OnLoad);
        image.Freeze();
        return image;
    }

    public void UpdatePreview(Size previewSize)
    {
        ViewPortSize = previewSize;
    }
}
