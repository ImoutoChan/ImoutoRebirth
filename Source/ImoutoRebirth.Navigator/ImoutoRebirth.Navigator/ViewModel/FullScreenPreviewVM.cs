using System.Windows;
using System.Windows.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ImoutoRebirth.Navigator.Services.Tags.Model;
using ImoutoRebirth.Navigator.UserControls;
using ImoutoRebirth.Navigator.ViewModel.ListEntries;

namespace ImoutoRebirth.Navigator.ViewModel;

internal partial class FullScreenPreviewVM : ObservableObject
{
    private readonly Func<INavigatorListEntry, Task<List<DelayItem>?>> _ugoiraDelaysGetter;
    private readonly Func<INavigatorListEntry, Task<IReadOnlyCollection<FileNote>>> _notesGetter;
    private IList<INavigatorListEntry>? _list;
    private BitmapImage? _bitmapImage;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(PngPath))]
    [NotifyPropertyChangedFor(nameof(GifPath))]
    [NotifyPropertyChangedFor(nameof(VideoPath))]
    [NotifyPropertyChangedFor(nameof(UgoiraPath))]
    [NotifyPropertyChangedFor(nameof(ImagePath))]
    [NotifyPropertyChangedFor(nameof(WebPPath))]
    private string? _path;

    [ObservableProperty]
    private INavigatorListEntry? _currentEntry;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(PngPath))]
    [NotifyPropertyChangedFor(nameof(GifPath))]
    [NotifyPropertyChangedFor(nameof(VideoPath))]
    [NotifyPropertyChangedFor(nameof(UgoiraPath))]
    [NotifyPropertyChangedFor(nameof(ImagePath))]
    [NotifyPropertyChangedFor(nameof(WebPPath))]
    private ListEntryType? _type;

    [ObservableProperty]
    private IReadOnlyCollection<DelayItem>? _ugoiraFrameDelays;
    
    [ObservableProperty]
    private IReadOnlyCollection<FileNote> _fileNotes = [];

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Zoom))]
    private Size _viewPortSize;

    public FullScreenPreviewVM(
        Func<INavigatorListEntry, Task<List<DelayItem>?>> ugoiraDelaysGetter, 
        Func<INavigatorListEntry, Task<IReadOnlyCollection<FileNote>>> notesGetter)
    {
        _ugoiraDelaysGetter = ugoiraDelaysGetter;
        _notesGetter = notesGetter;
    }

    public string? PngPath => Type == ListEntryType.Png ? Path : null;
    public string? GifPath => Type == ListEntryType.Gif ? Path : null;
    public string? VideoPath => Type == ListEntryType.Video ? Path : null;
    public string? UgoiraPath => Type == ListEntryType.Ugoira ? Path : null;
    public string? ImagePath => Type == ListEntryType.Image ? Path : null;
    public string? WebPPath => Type == ListEntryType.WebP ? Path : null;

    public double Zoom => ViewPortSize.Width / _bitmapImage?.PixelWidth ?? 1;

    public async void SetCurrentEntry(INavigatorListEntry forEntry, IList<INavigatorListEntry> list)
    {
        if (Type != forEntry.Type)
        {
            Type = null;
            Path = null;

            Path = forEntry.Path;
            Type = forEntry.Type;
        }
        else
        {
            Path = forEntry.Path;
        }

        if (Type is ListEntryType.Image or ListEntryType.Png)
            _bitmapImage = new BitmapImage(new Uri(Path));

        CurrentEntry = forEntry;
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
