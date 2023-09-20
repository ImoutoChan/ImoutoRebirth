using System.Windows.Input;
using ImoutoRebirth.Navigator.Commands;
using ImoutoRebirth.Navigator.ViewModel.ListEntries;

namespace ImoutoRebirth.Navigator.ViewModel;

internal class FullScreenPreviewVM : VMBase
{
    private string? _imagePath;
    private string? _pngPath;
    private string? _gifPath;
    private string? _videoPath;
    private string? _ugoiraPath;
    private bool _isImage;
    private bool _isPng;
    private bool _isGif;
    private bool _isVideo;
    private bool _isUgoira;

    public FullScreenPreviewVM(INavigatorListEntry forEntry)
    {
        switch (forEntry.Type)
        {
            case ListEntryType.Video:
                _isVideo = true;
                _videoPath = forEntry.Path;
                break;
            case ListEntryType.Gif:
                _isGif = true;
                _gifPath = forEntry.Path;
                break;
            case ListEntryType.Image:
                _isImage = true;
                _imagePath = forEntry.Path;
                break;
            case ListEntryType.Ugoira:
                _isUgoira = true;
                _ugoiraPath = forEntry.Path;
                break;
            case ListEntryType.Png:
                _isPng = true;
                _pngPath = forEntry.Path;
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
    
    public string? ImagePath 
    { 
        get => _imagePath; 
        set => OnPropertyChanged(ref _imagePath, value, () => ImagePath);
    }

    public string? PngPath 
    { 
        get => _pngPath; 
        set => OnPropertyChanged(ref _pngPath, value, () => PngPath);
    }

    public string? GifPath 
    { 
        get => _gifPath; 
        set => OnPropertyChanged(ref _gifPath, value, () => GifPath);
    }

    public string? VideoPath 
    { 
        get => _videoPath; 
        set => OnPropertyChanged(ref _videoPath, value, () => VideoPath);
    }

    public string? UgoiraPath 
    { 
        get => _ugoiraPath; 
        set => OnPropertyChanged(ref _ugoiraPath, value, () => UgoiraPath);
    }

    public bool IsImage 
    { 
        get => _isImage; 
        set => OnPropertyChanged(ref _isImage, value, () => IsImage);
    }

    public bool IsPng 
    { 
        get => _isPng; 
        set => OnPropertyChanged(ref _isPng, value, () => IsPng);
    }

    public bool IsGif 
    { 
        get => _isGif; 
        set => OnPropertyChanged(ref _isGif, value, () => IsGif);
    }

    public bool IsVideo 
    { 
        get => _isVideo; 
        set => OnPropertyChanged(ref _isVideo, value, () => IsVideo);
    }

    public bool IsUgoira 
    { 
        get => _isUgoira; 
        set => OnPropertyChanged(ref _isUgoira, value, () => IsUgoira);
    }
    
    public ICommand CloseCommand => new RelayCommand(_ => OnCloseRequested());

    public event EventHandler CloseRequested;

    protected virtual void OnCloseRequested() => CloseRequested?.Invoke(this, EventArgs.Empty);
}
