﻿using System.Windows.Input;
using ImoutoRebirth.Navigator.Commands;
using ImoutoRebirth.Navigator.UserControls;
using ImoutoRebirth.Navigator.ViewModel.ListEntries;

namespace ImoutoRebirth.Navigator.ViewModel;

internal class FullScreenPreviewVM : VMBase
{
    private readonly Func<INavigatorListEntry, Task<List<DelayItem>>> _ugoiraDelaysGetter;
    private string? _path;
    private INavigatorListEntry? _currentEntry;
    private ListEntryType? _type;
    private IList<INavigatorListEntry>? _list;
    private IReadOnlyCollection<DelayItem>? _ugoiraFrameDelays;

    public FullScreenPreviewVM(Func<INavigatorListEntry, Task<List<DelayItem>>> ugoiraDelaysGetter) 
        => _ugoiraDelaysGetter = ugoiraDelaysGetter;

    public string? PngPath => _type == ListEntryType.Png ? Path : null;
    public string? GifPath => _type == ListEntryType.Gif ? Path : null;
    public string? VideoPath => _type == ListEntryType.Video ? Path : null;
    public string? UgoiraPath => _type == ListEntryType.Ugoira ? Path : null;
    public string? ImagePath => _type == ListEntryType.Image ? Path : null;

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
    
    public async void SetCurrentEntry(INavigatorListEntry forEntry, IList<INavigatorListEntry> list)
    {
        Type = forEntry.Type;
        Path = forEntry.Path;
        CurrentEntry = forEntry;
        _list = list;

        if (Type == ListEntryType.Ugoira)
            UgoiraFrameDelays = await _ugoiraDelaysGetter(forEntry);
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
}