using System.IO;
using System.IO.Compression;
using System.Windows;
using System.Windows.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using ImoutoRebirth.Common;
using ImoutoRebirth.Lilin.WebApi.Client;
using SharpCompress.Archives;

namespace ImoutoRebirth.Navigator.ViewModel.ListEntries;

internal partial class DodjiEntryVM : BaseEntryVM, INavigatorListEntry, IPixelSizable
{
    private bool _isLoaded = false;

    [ObservableProperty]
    private Size _viewPortSize;

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private BitmapSource? _image;

    public DodjiEntryVM(
        string path,
        FilesClient filesClient,
        Size viewPortSize,
        Guid? dbId)
        : base(dbId, filesClient)
    {
        DbId = dbId;
        ViewPortSize = viewPortSize;
        Path = path;
    }

    public ListEntryType Type => ListEntryType.Dodji;

    public Guid? DbId { get; }

    public string Path { get; }

    public Size? PixelSize => Image != null ? new Size(Image.PixelWidth, Image.PixelHeight) : null;

    public object Data => new DataObject(DataFormats.FileDrop, new[] { Path });

    public DragDropEffects AllowDragDropEffects => DragDropEffects.Copy;

    public async void Load()
    {
        if (_isLoaded)
            return;

        IsLoading = true;

        var ratingTask = LoadRating();

        using var archive = ArchiveFactory.Open(Path);
        var preview = archive.Entries
          .FirstOrDefault(
              x => x is
                   {
                       Size: > 0,
                       IsDirectory: false
                   }
                   && (x.Key.EndsWithIgnoreCase(".jpg")
                       || x.Key.EndsWithIgnoreCase(".jpeg")
                       || x.Key.EndsWithIgnoreCase(".png")))
            ?? archive.Entries.FirstOrDefault(x => x is { Size: > 0, IsDirectory: false });

        if (preview == null)
            return;

        await using var previewStream = preview.OpenEntryStream();
        await using var resultImage = new MemoryStream();
        await previewStream.CopyToAsync(resultImage);
        resultImage.Seek(0, SeekOrigin.Begin);

        Image = BitmapFrame.Create(resultImage, BitmapCreateOptions.None, BitmapCacheOption.OnLoad);

        IsLoading = false;
        _isLoaded = true;

        await ratingTask;
    }

    public void UpdatePreview(Size previewSize)
    {
        ViewPortSize = previewSize;
    }
}
