using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using ImoutoRebirth.Common.WPF;
using ImoutoRebirth.Common.WPF.Commands;
using ImoutoRebirth.Navigator.Services.Tags.Model;
using ImoutoRebirth.Navigator.UserControls;
using ImoutoRebirth.Navigator.ViewModel.ListEntries;

namespace ImoutoRebirth.Navigator.ViewModel;

internal class FullScreenPreviewVM : VMBase
{
    private readonly Func<INavigatorListEntry, Task<List<DelayItem>?>> _ugoiraDelaysGetter;
    private readonly Func<INavigatorListEntry, Task<IReadOnlyCollection<FileNote>>> _notesGetter;
    private string? _path;
    private INavigatorListEntry? _currentEntry;
    private ListEntryType? _type;
    private IList<INavigatorListEntry>? _list;
    private IReadOnlyCollection<DelayItem>? _ugoiraFrameDelays;
    private IReadOnlyCollection<FileNote> _fileNotes = [];
    private Size _viewPortSize;
    private BitmapImage? _bitmapImage;

    public FullScreenPreviewVM(
        Func<INavigatorListEntry, Task<List<DelayItem>?>> ugoiraDelaysGetter, 
        Func<INavigatorListEntry, Task<IReadOnlyCollection<FileNote>>> notesGetter)
    {
        _ugoiraDelaysGetter = ugoiraDelaysGetter;
        _notesGetter = notesGetter;
    }

    public string? PngPath => _type == ListEntryType.Png ? Path : null;
    public string? GifPath => _type == ListEntryType.Gif ? Path : null;
    public string? VideoPath => _type == ListEntryType.Video ? Path : null;
    public string? UgoiraPath => _type == ListEntryType.Ugoira ? Path : null;
    public string? ImagePath => _type == ListEntryType.Image ? Path : null;
    public string? WebPPath => _type == ListEntryType.WebP ? Path : null;

    private string? Path
    {
        get => _path;
        set
        {
            OnPropertyChanged(ref _path, value, () => Path);
            OnPropertyChanged(nameof(PngPath));
            OnPropertyChanged(nameof(GifPath));
            OnPropertyChanged(nameof(VideoPath));
            OnPropertyChanged(nameof(UgoiraPath));
            OnPropertyChanged(nameof(ImagePath));
            OnPropertyChanged(nameof(WebPPath));
        }
    }
    
    private INavigatorListEntry? CurrentEntry
    {
        get => _currentEntry;
        set => OnPropertyChanged(ref _currentEntry, value, () => CurrentEntry);
    }

    public ListEntryType? Type
    {
        get => Path != null ? _type : null; 
        private set => OnPropertyChanged(ref _type, value, () => Type);
    }
    
    public IReadOnlyCollection<DelayItem>? UgoiraFrameDelays
    {
        get => _ugoiraFrameDelays;
        set => OnPropertyChanged(ref _ugoiraFrameDelays, value, () => UgoiraFrameDelays);
    }
    
    public IReadOnlyCollection<FileNote> FileNotes
    {
        get => _fileNotes;
        set => OnPropertyChanged(ref _fileNotes, value, () => FileNotes);
    }

    public Size ViewPortSize
    {
        get => _viewPortSize;
        set
        {
            OnPropertyChanged(ref _viewPortSize, value, () => ViewPortSize);
            OnPropertyChanged(nameof(Zoom));
        }
    }

    public double Zoom => ViewPortSize.Width / _bitmapImage?.PixelWidth ?? 1;

    public async void SetCurrentEntry(INavigatorListEntry forEntry, IList<INavigatorListEntry> list)
    {
        Type = forEntry.Type;
        Path = forEntry.Path;

        if (Type is ListEntryType.Image or ListEntryType.Png)
            _bitmapImage = new BitmapImage(new Uri(Path));

        CurrentEntry = forEntry;
        _list = list;

        var notesTask = _notesGetter(forEntry);

        if (Type == ListEntryType.Ugoira)
            UgoiraFrameDelays = await _ugoiraDelaysGetter(forEntry);

        var notes = await notesTask;
        FileNotes = notes.GroupBy(x => x.Source).MinBy(x => x.Key)?.ToList() ?? [];
        
        CurrentEntryNameChanged?.Invoke(this, System.IO.Path.GetFileName(forEntry.Path));
    }
    
    public ICommand CloseCommand => new RelayCommand(_ => CloseRequested?.Invoke(this, EventArgs.Empty));
    
    public ICommand NextPreviewCommand => new RelayCommand<bool>(isNext =>
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
    });

    public event EventHandler? CloseRequested;
    
    public event EventHandler<string>? CurrentEntryNameChanged;
}
