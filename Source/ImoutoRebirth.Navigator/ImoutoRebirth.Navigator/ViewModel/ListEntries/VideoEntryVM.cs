using System.Windows;
using System.Windows.Media.Imaging;
using ImoutoRebirth.Lilin.WebApi.Client;
using Microsoft.WindowsAPICodePack.Shell;

namespace ImoutoRebirth.Navigator.ViewModel.ListEntries;

internal class VideoEntryVM: BaseEntryVM, INavigatorListEntry
{
    #region Fields

    private Size _size;
    private bool _isLoading;
    private bool _isPreviewLoaded;
    private readonly object _loaderLocker = new();
    private bool _shouldPause;

    #endregion Fields

    #region Constructors

    public VideoEntryVM(
        string path,
        FilesClient filesClient,
        Size initPreviewSize,
        Guid? dbId)
        : base(dbId, filesClient)
    {
        Path = path;
        Type = ListEntryType.Video;
        ViewPortSize = initPreviewSize;
        DbId = dbId;
    }

    #endregion Constructors

    #region Properties

    public bool IsLoading
    {
        get => _isLoading;
        private set
        {
            OnPropertyChanged(ref _isLoading, value, () => IsLoading);
        }
    }

    public bool ShouldPause
    {
        get => _shouldPause;
        private set
        {
            OnPropertyChanged(ref _shouldPause, value, () => ShouldPause);
        }
    }

    public BitmapSource? Image { get; private set; }

    public Size ViewPortSize
    {
        get => _size;
        private set
        {
            _size = value;
            OnPropertyChanged(() => ViewPortSize);
        }
    }

    public string Path { get; }

    public ListEntryType Type { get; }

    public Guid? DbId { get; }

    #endregion Properties

    #region Public methods

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

    #endregion Public methods

    #region Private methods

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
        OnPropertyChanged(() => Image);
    }

    #endregion Private methods

    #region IDragable members

    public object Data => new DataObject(DataFormats.FileDrop, new[] { Path });

    public DragDropEffects AllowDragDropEffects => DragDropEffects.Copy;

    #endregion IDragable members
}
