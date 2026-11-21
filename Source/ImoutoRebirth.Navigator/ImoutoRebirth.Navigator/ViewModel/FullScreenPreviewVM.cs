using System.IO;
using System.Windows;
using System.Windows.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ImoutoRebirth.Navigator.Behaviors;
using ImoutoRebirth.Navigator.Model;
using ImoutoRebirth.Navigator.Services.Tags.Model;
using ImoutoRebirth.Navigator.UserControls;
using ImoutoRebirth.Navigator.ViewModel.ListEntries;
using Serilog;

namespace ImoutoRebirth.Navigator.ViewModel;

internal partial class FullScreenPreviewVM : ObservableObject, IDragable
{
    private readonly Func<INavigatorListEntry, Task<List<DelayItem>?>> _ugoiraDelaysGetter;
    private readonly Func<INavigatorListEntry, Task<IReadOnlyCollection<FileNote>>> _notesGetter;
    private IList<INavigatorListEntry>? _list;
    private BitmapImage? _bitmapImage;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(PngPath))]
    [NotifyPropertyChangedFor(nameof(GifSourceUri))]
    [NotifyPropertyChangedFor(nameof(VideoPath))]
    [NotifyPropertyChangedFor(nameof(UgoiraPath))]
    [NotifyPropertyChangedFor(nameof(ImagePath))]
    [NotifyPropertyChangedFor(nameof(WebPPath))]
    public partial string? Path { get; set; }

    [ObservableProperty]
    public partial INavigatorListEntry? CurrentEntry { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(PngPath))]
    [NotifyPropertyChangedFor(nameof(GifSourceUri))]
    [NotifyPropertyChangedFor(nameof(VideoPath))]
    [NotifyPropertyChangedFor(nameof(UgoiraPath))]
    [NotifyPropertyChangedFor(nameof(ImagePath))]
    [NotifyPropertyChangedFor(nameof(WebPPath))]
    public partial ListEntryType? Type { get; set; }

    [ObservableProperty]
    public partial IReadOnlyCollection<DelayItem>? UgoiraFrameDelays { get; set; }

    [ObservableProperty]
    private IReadOnlyCollection<FileNote> _fileNotes = [];

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Zoom))]
    public partial Size ViewPortSize { get; set; }

    public FullScreenPreviewVM(
        Func<INavigatorListEntry, Task<List<DelayItem>?>> ugoiraDelaysGetter, 
        Func<INavigatorListEntry, Task<IReadOnlyCollection<FileNote>>> notesGetter)
    {
        _ugoiraDelaysGetter = ugoiraDelaysGetter;
        _notesGetter = notesGetter;
    }

    public string? PngPath => Type == ListEntryType.Png ? Path : null;

    public Uri? GifSourceUri => Type == ListEntryType.Gif && Uri.TryCreate(Path, UriKind.Absolute, out var uri)
        ? uri
        : null;

    public string? VideoPath => Type == ListEntryType.Video ? Path : null;
    public string? UgoiraPath => Type == ListEntryType.Ugoira ? Path : null;
    public string? ImagePath => Type == ListEntryType.Image ? Path : null;
    public string? WebPPath => Type == ListEntryType.WebP ? Path : null;

    public double Zoom => ViewPortSize.Width / _bitmapImage?.PixelWidth ?? 1;

    public object Data => CurrentEntry?.Data ?? new DataObject();
    public DragDropEffects AllowDragDropEffects => DragDropEffects.Copy;

    public async void SetCurrentEntry(INavigatorListEntry forEntry, IList<INavigatorListEntry> list)
    {
        var type = forEntry.Type;
        string? path = forEntry.Path;
        _bitmapImage = null;
        if (type is ListEntryType.Image or ListEntryType.Png)
        {
            try
            {
                if (type == ListEntryType.Png && !PngAnimation.IsAnimated(path))
                    type = ListEntryType.Image;

                _bitmapImage = new BitmapImage(new Uri(path));
            }
            catch (Exception e) when (e is IOException or UnauthorizedAccessException or FormatException or NotSupportedException)
            {
                Log.Warning(e, "Unable to read fullscreen preview {Path}", path);
                path = null;
            }
        }

        if (Type != type)
        {
            Type = null;
            Path = null;

            Path = path;
            Type = type;
        }
        else
        {
            Path = path;
        }

        CurrentEntry = forEntry;
        if (forEntry is DodjiEntryVM archive)
            archive.Load();
        _list = list;

        var notesTask = _notesGetter(forEntry);

        if (Type == ListEntryType.Ugoira)
        {
            UgoiraFrameDelays = await _ugoiraDelaysGetter(forEntry);
        }
        else
        {
            UgoiraFrameDelays = [];
        }

        var notes = await notesTask;
        FileNotes = notes.GroupBy(x => x.Source).MinBy(x => x.Key)?.ToList() ?? [];
        
        CurrentEntryNameChanged?.Invoke(this, System.IO.Path.GetFileName(forEntry.Path));
    }
    
    [RelayCommand]
    private void NextPreview(bool isNext)
    {
        var current = CurrentEntry;
        var list = _list;
        
        if (current == null || list == null)
            return;
        
        var currentIndex = list.IndexOf(current);
        var nextIndex = currentIndex + 1;
        var prevIndex = currentIndex - 1;
        var nextEntry = nextIndex < list.Count ? list[nextIndex] : list[0];
        var prevEntry = prevIndex >= 0 ? list[prevIndex] : list[^1];

        SetCurrentEntry(isNext ? nextEntry : prevEntry, list);
    }

    [RelayCommand]
    private void Close() => CloseRequested?.Invoke(this, EventArgs.Empty);

    public event EventHandler? CloseRequested;
    
    public event EventHandler<string>? CurrentEntryNameChanged;
}
