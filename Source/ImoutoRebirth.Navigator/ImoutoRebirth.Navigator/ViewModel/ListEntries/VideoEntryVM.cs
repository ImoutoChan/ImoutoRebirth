using System.Windows;
using System.Windows.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using ImoutoRebirth.Lilin.WebApi.Client;
using Microsoft.WindowsAPICodePack.Shell;

namespace ImoutoRebirth.Navigator.ViewModel.ListEntries;

internal partial class VideoEntryVM: BaseEntryVM, INavigatorListEntry
{
    private bool _isPreviewLoaded;
    private readonly object _loaderLocker = new();

    [ObservableProperty]
    public partial Size ViewPortSize { get; set; }

    [ObservableProperty]
    public partial bool IsLoading { get; set; }

    [ObservableProperty]
    public partial bool ShouldPause { get; set; }

    [ObservableProperty]
    private BitmapSource? _image;

    public VideoEntryVM(string path, FilesClient filesClient, Size initPreviewSize, Guid? dbId)
        : base(dbId, filesClient)
    {
        Path = path;
        Type = ListEntryType.Video;
        ViewPortSize = initPreviewSize;
        DbId = dbId;
    }

    public string Path { get; }

    public ListEntryType Type { get; }

    public Guid? DbId { get; }

    public void UpdatePreview(Size previewSize)
    {
        ViewPortSize = previewSize;
    }

    public async void Load()
    {
        LoadPreview();
        await LoadRating();
    }

    public void Pause()
    {
        ShouldPause = false;
        ShouldPause = true;
    }

    private async void LoadPreview()
    {
        lock (_loaderLocker)
        {
            if (_isPreviewLoaded || IsLoading)
            {
                return;
            }

            IsLoading = true;
        }

        var thumbNail = await Task.Run(() =>
        {
            using var shellFile = ShellFile.FromFilePath(Path);
            var thumbnail = shellFile.Thumbnail.BitmapSource;
            thumbnail.Freeze();
            return thumbnail;
        });

        Image = thumbNail;
        _isPreviewLoaded = true;
        IsLoading = false;
    }

    public object Data => new DataObject(DataFormats.FileDrop, new[] { Path });

    public DragDropEffects AllowDragDropEffects => DragDropEffects.Copy;
}
