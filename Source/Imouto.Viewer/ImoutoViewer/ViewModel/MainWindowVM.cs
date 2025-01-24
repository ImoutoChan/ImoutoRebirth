using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using AutoMapper;
using ImoutoRebirth.Common;
using ImoutoRebirth.Common.WPF;
using ImoutoViewer.Behavior;
using ImoutoRebirth.Common.WPF.Commands;
using ImoutoViewer.ImoutoRebirth.Services;
using ImoutoViewer.ImoutoRebirth.Services.Tags.Model;
using ImoutoViewer.Model;
using ImoutoViewer.Model.ArchiveSupport;
using ImoutoViewer.ViewModel.SettingsModels;

namespace ImoutoViewer.ViewModel;

internal class MainWindowVM : VMBase, IDragable, IDropable
{
    #region Fields

    private readonly Guid _appGuid = Guid.NewGuid();

    private readonly MainWindow _mainWindowView;
    private ILocalImageList? _imageList;
    private bool _isSlideShowActive;
    private DispatcherTimer? _timer;
    private readonly TagsVM _tagsVM;
    private readonly AddTagVM _addTagVM;
    private readonly CreateTagVM _createTagVM;
    private readonly IFileLoadingService _fileLoadingService;
    private readonly IMapper _mapper;

    #endregion Fields

    #region Constructors

    public MainWindowVM()
    {
        _fileLoadingService = ServiceLocator.GetRequiredService<IFileLoadingService>();
        _mapper = ServiceLocator.GetRequiredService<IMapper>();

        OpenWith = new OpenWithVM();
        IsSimpleWheelNavigationEnable = true;

        _mainWindowView = new MainWindow { DataContext = this };
        _mainWindowView.SizeChanged += _mainWindowView_SizeChanged;
        _mainWindowView.Client.SizeChanged += _mainWindowView_SizeChanged;
        _tagsVM = new TagsVM(this);
        _addTagVM = new AddTagVM(this);
        _createTagVM = new CreateTagVM(this);

        InitializeCommands();
        InitializeSettings();

        _mainWindowView.Show();

        OnPropertyChanged(() => Settings);

        InitializeImageListAsync();

        View.ViewPort.SizeChanged += (_, _) => UpdateView();
    }

    #endregion Constructors

    #region Properties

    public MainWindow View => _mainWindowView;

    public AddTagVM AddTagVM => _addTagVM;

    public CreateTagVM CreateTagVM => _createTagVM;

    public TagsVM Tags => _tagsVM;

    public bool IsSlideShowActive
    {
        get => _isSlideShowActive;
        set
        {
            if (_isSlideShowActive)
            {
                _timer?.Stop();
            }
            if (value)
            {
                _timer = new DispatcherTimer { Interval = new TimeSpan(0, 0, 0, 0, Settings.SlideshowDelay) };
                _timer.Tick += (_, _) => NextImage();
                _timer.Start();
            }

            _isSlideShowActive = value;
            OnPropertyChanged(nameof(IsSlideShowActive));
        }
    }

    public LocalImage? CurrentLocalImage
    {
        get
        {
            var bs = _imageList?.CurrentImage?.Image;
            if (IsError || bs == null)
            {
                return null;
            }
            return _imageList?.CurrentImage;
        }
    }

    public string Title
    {
        get
        {
            string title = "";

            if (CurrentLocalImage != null && _imageList != null && !_imageList.IsEmpty)
            {
                title += CurrentLocalImage.Name + " - ";
            }

            title += System.Reflection.Assembly.GetExecutingAssembly().GetName().Name?.Replace('.', ' ');

            return title;
        }
    }

    public string Status
    {
        get
        {
            if (IsError)
                return "Image loading error";

            if (IsLoading)
                return "Loading...";

            return "Ready";
        }
    }

    public string? DirStatus
    {
        get
        {
            if (_imageList == null)
            {
                return null;
            }

            int i = (int)Math.Log10(_imageList.DirectoriesCount + 1) + 1;
            var template = "{0," + i + "} / {1," + i + "}";
            return string.Format(template,
                _imageList.CurrentDirectoryIndex + 1,
                _imageList.DirectoriesCount);
        }
    }

    public string? DirStatusToolTip => _imageList?.CurrentDirectory?.Name;

    public string? FileStatus
    {
        get
        {
            if (_imageList == null)
                return null;

            var i = (int)Math.Log10(_imageList.ImagesCount) + 1;
            var template = "{0," + i + "} / {1," + i + "}";
            
            return string.Format(template,
                _imageList.CurrentImageIndex + 1,
                _imageList.ImagesCount);
        }
    }

    public string ZoomString =>
        CurrentLocalImage != null
            ? $"{CurrentLocalImage.Zoom * 100:N0} %"
            : "100 %";

    public double Zoom => CurrentLocalImage?.Zoom ?? 1;

    public BitmapSource? Image =>
        CurrentLocalImage != null && CurrentLocalImage.ImageFormat != ImageFormat.GIF
            ? CurrentLocalImage.Image
            : null;

    public BitmapSource? AnimutedImage =>
        CurrentLocalImage is { ImageFormat: ImageFormat.GIF }
            ? CurrentLocalImage.Image
            : null;

    public bool IsAnimuted => CurrentLocalImage is { ImageFormat: ImageFormat.GIF };

    public bool IsLoading { get; private set; }

    public int LoadingProgress { get; private set; } = 100;

    public double ViewportHeight => CurrentLocalImage?.ResizedSize.Height ?? 0;

    public double ViewportWidth => CurrentLocalImage?.ResizedSize.Width ?? 0;

    public bool IsSimpleWheelNavigationEnable { private get; set; }

    public SettingsVM Settings { get; private set; }

    public OpenWithVM OpenWith { get; private set; }

    public bool IsError => _imageList?.CurrentImage is { IsError: true };

    public string? ErrorMessage => _imageList?.CurrentImage?.ErrorMessage;

    public string ImagePath => CurrentLocalImage?.Path ?? "";

    public bool IsZoomFixed => LocalImage.IsZoomFixed;

    #endregion Properties

    #region Methods

    [MemberNotNull(nameof(Settings))]
    private void InitializeSettings()
    {
        Settings = new SettingsVM();
        Settings.SelectedResizeTypeChanged += _settings_SelectedResizeTypeChanged;
        Settings.SelectedDirectorySearchTypeChanged += _settings_SelectedDirectorySearchTypeChanged;
        Settings.SelectedFilesSortingChanged += Settings_SelectedFilesSortingChanged;
        Settings.SelectedFoldersSortingChanged += Settings_SelectedFoldersSortingChanged;
        Settings.SelectedTagsModeChanged += Settings_SelectedTagsModeChanged;
        Settings.SelectedNotesModeChanged += Settings_SelectedNotesModeChanged;

        LocalImageList.FilesGettingMethods = Settings.DirectorySearchFlags;
        LocalImageList.FilesSortMethod = Settings.SelectedFilesSorting.Method;
        LocalImageList.IsFilesSortMethodDescending = Settings.IsSelectedFilesSortingDescending;
        LocalImageList.FoldersSortMethod = Settings.SelectedFoldersSorting.Method;
        LocalImageList.IsFoldersSortMethodDescending = Settings.IsSelectedFoldersSortingDescending;
        Tags.ShowTags = Settings.ShowTags;
        Tags.ShowNotes = Settings.ShowNotes;
    }

    private void Settings_SelectedNotesModeChanged(object? sender, EventArgs e)
    {
        Tags.ShowNotes = Settings.ShowNotes;
    }

    private void Settings_SelectedTagsModeChanged(object? sender, EventArgs e)
    {
        Tags.ShowTags = Settings.ShowTags;
    }

    private async Task InitializeImageList(IReadOnlyCollection<string>? images)
    {
        TemporaryDirectoryManager.CleanupOldTempDirectories();
        if (_imageList != null)
        {
            _imageList.CurrentImageChanged -= _imageList_CurrentImageChanged;
            _imageList?.Dispose();
            _imageList = null;
            OnPropertyChanged(nameof(CurrentLocalImage));
            UpdateView();
        }

        ApplicationProperties.BoundToNavigatorSearch = false;

        ILocalImageList imageList;

        if (images?.Count == 1 && ArchiveImageList.IsSupportedArchive(images.First()))
        {
            imageList = new ArchiveImageList(images.First(), progress =>
            {
                LoadingProgress = (int)(progress * 100);
                UpdateView();
            });
        }
        else if (images != null)
        {
            imageList = new LocalImageList(images);
        }
        else if (ApplicationProperties.NavigatorSearchParams != null)
        {
            ApplicationProperties.BoundToNavigatorSearch = true;

            var searchParams = ApplicationProperties.NavigatorSearchParams;

            try
            {
                var files = await _fileLoadingService.LoadFiles(
                    searchParams.CollectionId,
                    _mapper.Map<IReadOnlyCollection<SearchTag>>(searchParams.SearchTags));

                if (files.Any())
                {
                    imageList = new LocalImageList(files, -1, ApplicationProperties.FileNamesToOpen?.FirstOrDefault());
                }
                else
                {
                    imageList = new LocalImageList();
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
                imageList = new LocalImageList();
            }

        }
        else if (ApplicationProperties.FileNamesToOpen?.Any() == true)
        {
            var fileNames = ApplicationProperties.FileNamesToOpen;
            ApplicationProperties.FileNamesToOpen = ArraySegment<string>.Empty;

            if (fileNames.Count == 1 && ArchiveImageList.IsSupportedArchive(fileNames.First()))
            {
                imageList = new ArchiveImageList(fileNames.First(), progress =>
                {
                    LoadingProgress = (int)(progress * 100);
                    UpdateView();
                });
            }
            else
            {
                imageList = new LocalImageList(fileNames);
            }
        }
        else
        {
            imageList = new LocalImageList();
        }

        imageList.CurrentImageChanged += _imageList_CurrentImageChanged;
        _imageList = imageList;
    }

    private void UpdateView()
    {
        try
        {
            _imageList?.CurrentImage?.Resize(_mainWindowView.Client.RenderSize, Settings.SelectedResizeType.Type);

            OnPropertyChanged("Title");
            OnPropertyChanged("ViewportHeight");
            OnPropertyChanged("ViewportWidth");
            OnPropertyChanged("AnimutedImage");
            OnPropertyChanged("Image");
            OnPropertyChanged("IsAnimuted");
            OnPropertyChanged("IsError");
            OnPropertyChanged("ErrorMessage");
            OnPropertyChanged("DirStatus");
            OnPropertyChanged("FileStatus");
            OnPropertyChanged("DirStatusToolTip");
            OnPropertyChanged("Status");
            OnPropertyChanged("IsLoading");
            OnPropertyChanged("ZoomString");
            OnPropertyChanged("ImagePath");
            OnPropertyChanged("Zoom");
            OnPropertyChanged(nameof(LoadingProgress));

            if (_mainWindowView.ScrollViewerObject.IsNeedScrollHome)
            {
                _mainWindowView.ScrollViewerObject.ScrollToHome();
            }
        }
        catch (OutOfMemoryException)
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();

            UpdateView();
        }
        catch (Exception)
        {
            //Debug.WriteLine(ex.Message);
            //NextImage();
        }
    }

    private async void InitializeImageListAsync(IReadOnlyCollection<string>? images = null)
    {
        IsLoading = true;
        OnPropertyChanged("Status");
        OnPropertyChanged("IsLoading");

        await Task.Run(() => InitializeImageList(images));

        IsLoading = false;
        UpdateView();

        _tagsVM.ReloadAsync();
    }

    #endregion Methods

    #region Commands

    public ICommand? SimpleNextImageCommand { get; private set; }
    public ICommand? SimplePrevImageCommand { get; private set; }
    public ICommand? NextImageCommand { get; private set; }
    public ICommand? PrevImageCommand { get; private set; }
    public ICommand? ZoomInCommand { get; private set; }
    public ICommand? ZoomOutCommand { get; private set; }
    public ICommand? NextFolderCommand { get; private set; }
    public ICommand? PrevFolderCommand { get; private set; }
    public ICommand? FixZoomCommand { get; private set; }
    public ICommand? ToggleSlideshowCommand { get; private set; }

    /// <summary>
    /// Rotation the image. In CommandParameter as string send: 
    /// "right" to rotate image on 90 deg right; 
    /// "left to rotate image on -90 deg left.
    /// </summary>
    public ICommand? RotateCommand { get; private set; }
    public ICommand? ToggleTagsCommand { get; private set; }
    public ICommand? ToggleNotesCommand { get; private set; }

    [MemberNotNull(nameof(SimpleNextImageCommand))]
    [MemberNotNull(nameof(SimplePrevImageCommand))]
    [MemberNotNull(nameof(NextImageCommand))]
    [MemberNotNull(nameof(PrevImageCommand))]
    [MemberNotNull(nameof(ZoomInCommand))]
    [MemberNotNull(nameof(ZoomOutCommand))]
    [MemberNotNull(nameof(NextFolderCommand))]
    [MemberNotNull(nameof(PrevFolderCommand))]
    [MemberNotNull(nameof(FixZoomCommand))]
    [MemberNotNull(nameof(ToggleSlideshowCommand))]
    [MemberNotNull(nameof(RotateCommand))]
    [MemberNotNull(nameof(ToggleTagsCommand))]
    [MemberNotNull(nameof(ToggleNotesCommand))]
    private void InitializeCommands()
    {
        SimpleNextImageCommand = new RelayCommand(_ =>
        {
            if (IsSimpleWheelNavigationEnable)
            {
                NextImage();
            }
        });

        SimplePrevImageCommand = new RelayCommand(_ =>
        {
            if (IsSimpleWheelNavigationEnable)
            {
                PrevImage();
            }
        });

        NextImageCommand = new RelayCommand(_ => NextImage());
        PrevImageCommand = new RelayCommand(_ => PrevImage());

        ZoomInCommand = new RelayCommand(_ => ZoomIn());
        ZoomOutCommand = new RelayCommand(_ => ZoomOut());

        RotateCommand = new RelayCommand(Rotate);

        NextFolderCommand = new RelayCommand(_ => NextFolder());
        PrevFolderCommand = new RelayCommand(_ => PrevFolder());

        FixZoomCommand = new RelayCommand(_ => FixZoom());
        ToggleSlideshowCommand = new RelayCommand(ToggleSlideshow);
        ToggleTagsCommand = new RelayCommand(_ => ToggleTags());
        ToggleNotesCommand = new RelayCommand(_ => ToggleNotes());
    }

    #endregion Commands

    #region Command handlers
        
    private void ToggleNotes()
    {
        Settings.ShowNotes = !Settings.ShowNotes;
        Settings.SaveCommand.Execute(new object());
    }

    private void ToggleTags()
    {
        Settings.ShowTags = !Settings.ShowTags;
        Settings.SaveCommand.Execute(new object());
    }

    private void ToggleSlideshow(object? param)
    {
        if (param is "ForcedDisable")
        {
            IsSlideShowActive = false;
        }
        else
        {
            IsSlideShowActive = !IsSlideShowActive;
        }
    }

    private void NextImage()
    {
        _imageList?.Next();
        UpdateView();
    }

    private void PrevImage()
    {
        _imageList?.Previous();
        UpdateView();
    }

    private void ZoomIn()
    {
        var localImage = CurrentLocalImage;
        if (localImage == null)
            return;

        CurrentLocalImage?.ZoomIn();
        UpdateView();
    }

    private void ZoomOut()
    {
        CurrentLocalImage?.ZoomOut();
        UpdateView();
    }

    private void Rotate(object? param)
    {
        switch (param as string)
        {
            case "left":
                CurrentLocalImage?.RotateLeft();
                break;
            case "right":
                CurrentLocalImage?.RotateRight();
                break;
        }
        UpdateView();
    }

    private void NextFolder()
    {
        _imageList?.NextFolder();
        UpdateView();
    }

    private void PrevFolder()
    {
        _imageList?.PrevFolder();
        UpdateView();
    }

    private void FixZoom()
    {
        var localImage = CurrentLocalImage;
        if (localImage == null)
            return;
        
        LocalImage.StaticZoom = !LocalImage.IsZoomFixed ? localImage.Zoom : 1;
        LocalImage.IsZoomFixed = !LocalImage.IsZoomFixed;

        OnPropertyChanged("IsZoomFixed");
    }

    #endregion Command handlers

    #region Event handlers

    private void _mainWindowView_SizeChanged(object? sender, SizeChangedEventArgs e)
    {
        UpdateView();
    }

    private void _settings_SelectedResizeTypeChanged(object? sender, EventArgs e)
    {
        UpdateView();
    }

    private void _settings_SelectedDirectorySearchTypeChanged(object? sender, EventArgs e)
    {
        var localImage = CurrentLocalImage;
        if (localImage == null)
            return;
        
        LocalImageList.FilesGettingMethods = Settings.DirectorySearchFlags;
        InitializeImageListAsync(new[] { localImage.Path });
    }

    private void Settings_SelectedFoldersSortingChanged(object? sender, EventArgs e)
    {
        var localImage = CurrentLocalImage;
        if (localImage == null)
            return;

        LocalImageList.FoldersSortMethod = Settings.SelectedFoldersSorting.Method;
        LocalImageList.IsFoldersSortMethodDescending = Settings.IsSelectedFoldersSortingDescending;
        InitializeImageListAsync(new[] { localImage.Path });
    }

    private void Settings_SelectedFilesSortingChanged(object? sender, EventArgs e)
    {
        LocalImageList.FilesSortMethod = Settings.SelectedFilesSorting.Method;
        LocalImageList.IsFilesSortMethodDescending = Settings.IsSelectedFilesSortingDescending;
        _imageList?.ResortFiles();
        UpdateView();
    }

    private void _imageList_CurrentImageChanged(object? sender, EventArgs e)
    {
        _mainWindowView.ScrollViewerObject.IsNeedScrollHome = true;

        _tagsVM.ReloadAsync();
    }

    #endregion Event handlers

    #region IDragable members

    public object? Data
    {
        get
        {
            var localImage = CurrentLocalImage;
            if (localImage == null)
                return null;
            
            var data = new DataObject(DataFormats.FileDrop, new[] { localImage.Path });
            data.SetData("DragSource", _appGuid);
            return data;
        }
    }

    public DragDropEffects AllowDragDropEffects => DragDropEffects.Copy;

    #endregion IDragable members

    #region IDropable members

    public List<string> AllowDataTypes
    {
        get
        {
            var list = new List<string> { DataFormats.FileDrop };

            return list;
        }
    }

    public void Drop(object data, object? sourceGuid)
    {
        var droppedFiles = (string[])data;

        if (sourceGuid != null)
        {
            try
            {
                var typedSourceGuid = (Guid) sourceGuid;
                if (typedSourceGuid == _appGuid)
                {
                    return;
                }
            }
            catch
            {
                // ignore
            }
        }

        InitializeImageListAsync(droppedFiles);
    }

    #endregion IDropable members
}
