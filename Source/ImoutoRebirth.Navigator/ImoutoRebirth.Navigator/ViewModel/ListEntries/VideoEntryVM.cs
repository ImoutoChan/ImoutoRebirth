using System.Windows;
using System.Windows.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.WindowsAPICodePack.Shell;

namespace ImoutoRebirth.Navigator.ViewModel.ListEntries;

internal partial class VideoEntryVM: BaseEntryVM, INavigatorListEntry
{
    private bool _isPreviewLoaded;
    private readonly Lock _loaderLocker = new();

    [ObservableProperty]
    public partial Size ViewPortSize { get; set; }

    [ObservableProperty]
    public partial bool IsLoading { get; set; }

    [ObservableProperty]
    public partial bool ShouldPause { get; set; }

    [ObservableProperty]
    private BitmapSource? _image;

    public VideoEntryVM(string path, Size initPreviewSize, Guid? dbId)
        : base(dbId)
    {
        Path = path;
        Type = ListEntryType.Video;
        ViewPortSize = initPreviewSize;
    }

    public string Path { get; }

    public ListEntryType Type { get; }

    public void UpdatePreview(Size previewSize)
    {
        ViewPortSize = previewSize;
    }

    public void Load() => LoadPreview();

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
