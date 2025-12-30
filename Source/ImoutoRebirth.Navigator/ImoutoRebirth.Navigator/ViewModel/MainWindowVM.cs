using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using ImoutoRebirth.Common;
using ImoutoRebirth.Navigator.Model;
using ImoutoRebirth.Navigator.Services;
using ImoutoRebirth.Navigator.Services.ImoutoViewer;
using ImoutoRebirth.Navigator.Services.Tags;
using ImoutoRebirth.Navigator.Slices.CreateCollectionWizard;
using ImoutoRebirth.Navigator.Slices.QuickTagging;
using ImoutoRebirth.Navigator.UserControls;
using ImoutoRebirth.Navigator.Utils;
using ImoutoRebirth.Navigator.ViewModel.ListEntries;
using ImoutoRebirth.Navigator.ViewModel.SettingsSlice;
using MahApps.Metro.Controls.Dialogs;
using Newtonsoft.Json;
using Serilog;
using System.Collections;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Threading;
using ImoutoRebirth.Common.WPF;
using ObservableCollections;
using File = System.IO.File;
using ImoutoRebirth.Navigator.Slices.IntegrityReport.ViewModels;

namespace ImoutoRebirth.Navigator.ViewModel;

internal partial class MainWindowVM : ObservableObject
{
    private const string DefaultTitle = "Imouto Navigator";

    private readonly MainWindow _view;
    private readonly IFileService _fileService;
    private readonly IFileTagService _fileTagService;
    private readonly IFileNoteService _fileNoteService;
    private readonly IFileLoadingService _fileLoadingService;
    private readonly IImoutoViewerService _imoutoViewerService;
    private readonly IMessenger _messenger;
    private readonly DispatcherTimer _appendNewContentTimer = new() { Interval = TimeSpan.FromSeconds(5) };

    private int _previewSize = 256;

    [ObservableProperty]
    public partial bool ShowTags { get; set; } = true;

    [ObservableProperty]
    public partial bool ShowQuickTagging { get; set; } = false;

    [ObservableProperty]
    private int _volume = 100;

    [ObservableProperty]
    public partial string Title { get; set; }

    [ObservableProperty]
    public partial FullScreenPreviewVM? FullScreenPreviewVM { get; set; }

    [ObservableProperty]
    public partial QuickTaggingVM? QuickTagging { get; set; }

    [ObservableProperty]
    public partial int TotalCount { get; set; }

    [ObservableProperty]
    public partial bool IsLoading { get; set; }

    [ObservableProperty]
    public partial string? Status { get; set; }

    [ObservableProperty]
    public partial string? StatusToolTip { get; set; }

    public MainWindowVM()
    {
        _fileLoadingService = ServiceLocator.GetService<IFileLoadingService>();
        _fileService = ServiceLocator.GetService<IFileService>();
        _fileTagService = ServiceLocator.GetService<IFileTagService>();
        _fileNoteService = ServiceLocator.GetService<IFileNoteService>();
        _imoutoViewerService = ServiceLocator.GetService<IImoutoViewerService>();
        _messenger = ServiceLocator.GetService<IMessenger>();

        NavigatorListInternal = new();
        NavigatorListView
            = NavigatorListInternal.ToNotifyCollectionChanged(SynchronizationContextCollectionEventDispatcher.Current);

        NavigatorListInternal.CollectionChanged
            += (in _) => T.Debounce(100, () => OnPropertyChanged(nameof(LoadedCount)));

        _appendNewContentTimer.Tick += (_, _) => { /*LoadNew();*/ };
        Title = DefaultTitle;

        TagSearchVM = new TagSearchVM();
        CurrentFileTagsVM = new CurrentFileTagsVM();
        TagsEdit = new TagsEditVM(this);
        QuickTagging = new QuickTaggingVM(this);
        TagsMerge = new TagsMergeVM();
        
        TagSearchVM.SelectedTagsUpdated += TagSearchVM_SelectedTagsUpdated;
        TagSearchVM.SelectedCollectionChanged += TagSearchVMOnSelectedCollectionChanged;

        Settings.ShowPreviewOnSelectChanged += Settings_ShowPreviewOnSelectChanged;

        _view = new MainWindow { DataContext = this };
        _view.SelectedItemsChanged += OnViewOnSelectedItemsChanged;
        _view.Loaded += (_, _) => ZoomToRowElements(6);

        _messenger.Register<QuickTaggingCloseRequest>(this, (_, _) => ShowQuickTagging = false);
        _messenger.Register<OpenCreateCollectionWizardRequest>(this, (_, _) => OpenCreateCollectionWizard());
        _messenger.Register<RefreshCollectionsRequest>(this, (_, _) => InitializeAsync().LogAndSuppressExceptions());
    }

    public void ShowWindow() => _view.Show();

    private async Task InitializeAsync()
    {
        var topTagsUpdateTask = TagsEdit.UpdateUsersTopTags();
        await CollectionManager.ReloadCollectionsAsync();
        TagSearchVM.AddCollections(CollectionManager.Collections);
        await topTagsUpdateTask;
    }

    private void OnViewOnSelectedItemsChanged(object? sender, EventArgs args)
    {
        OnPropertyChanged(nameof(SelectedEntries));

        if (SelectedItem == null)
            return;

        CurrentFileTagsVM.UpdateCurrentTags(SelectedItem.DbId)
            .OnException(ex =>
            {
                App.MainWindowVM?.SetStatusError("Error while updating current tags", ex.Message);
                Log.Error(ex, "Error while updating current tags for {SelectedItemDbId}", SelectedItem.DbId);
            });
        FileInfoVM.UpdateCurrentInfo(SelectedItem, NavigatorListInternal.IndexOf(SelectedItem));
    }

    public Size SlotSize => new(_previewSize + 30, _previewSize + 30);

    private Size PreviewSize => new(_previewSize, _previewSize);

    public ObservableList<INavigatorListEntry> NavigatorListInternal { get; }

    public NotifyCollectionChangedSynchronizedViewList<INavigatorListEntry> NavigatorListView { get; }

    public TagSearchVM TagSearchVM { get; set; }

    public CurrentFileTagsVM CurrentFileTagsVM { get; set; }

    public CollectionManagerVm CollectionManager { get; } = new();

    public SettingsVM Settings { get; } = new();

    public IntegrityReportsVM IntegrityReports { get; } = new();

    public TagsEditVM TagsEdit { get; set; }

    public TagsMergeVM TagsMerge { get; set; }

    public int LoadedCount => NavigatorListInternal.Count;

    public bool ShowPreview => Settings.ShowPreviewOnSelect;

    public IEnumerable<INavigatorListEntry> SelectedEntries => _view.SelectedEntries;

    public IList SelectedItems => _view.SelectedItems;

    public FileInfoVM FileInfoVM { get; } = new();

    public INavigatorListEntry? SelectedItem => _view.MediaListBox.SelectedItem as INavigatorListEntry;

    [RelayCommand]
    private void RevertSelectedItems() => _view.RevertSelectedItems();

    [RelayCommand]
    private void ToggleShowTags() => ShowTags = !ShowTags;

    [RelayCommand]
    private void ZoomToRowElements(object? x)
    {
        if (x is not string param) 
            return;

        var toElements = int.Parse(param);
        var boxSize = _view.ViewPortWidth / toElements;
        _previewSize = (int)(boxSize - 30);

        UpdatePreviews();
    }

    [RelayCommand]
    private void ZoomOut()
    {
        if (_previewSize < 64)
        {
            return;
        }

        _previewSize = Convert.ToInt32(Math.Floor(_previewSize * 0.9));
        UpdatePreviews();
    }

    [RelayCommand]
    private void ZoomIn()
    {
        _previewSize = Convert.ToInt32(Math.Floor(_previewSize * 1.1));
        UpdatePreviews();
    }

    [RelayCommand]
    private void OpenFile(INavigatorListEntry? navigatorListEntry)
    {
        switch (navigatorListEntry)
        {
            case ImageEntryVM image:
                _imoutoViewerService.OpenFile(
                    image.Path,
                    TagSearchVM.SelectedCollection.Value,
                    TagSearchVM.SelectedBindedTags.Select(x => x.Model));
                break;
            case VideoEntryVM video:
            {
                video.Pause();

                var mpcHcExe = new FileInfo(@"C:\Program Files\MPC-HC\mpc-hc64.exe");
                if (mpcHcExe.Exists)
                {
                    var videos = NavigatorListInternal
                        .Where(x => x.Type == ListEntryType.Video)
                        .Select(x => x.Path)
                        .ToList();

                    var pathToPlaylist = CreatePlayList(videos, video.Path);

                    new Process
                    {
                        StartInfo = new ProcessStartInfo(mpcHcExe.FullName, @$"""{pathToPlaylist}"" /play /new")
                    }.Start();
                }
                else
                {
                    var process = new Process
                    {
                        StartInfo = new ProcessStartInfo(video.Path) { UseShellExecute = true }
                    };

                    process.Start();
                }
                break;
            }
            default:
            {
                Log.Warning("Unknown format {Type}", navigatorListEntry?.GetType().FullName);

                if (navigatorListEntry?.Path == null)
                    return;

                var process = new Process
                {
                    StartInfo = new ProcessStartInfo(navigatorListEntry.Path) { UseShellExecute = true }
                };

                process.Start();
            }
                break;
        }
    }
    
    [RelayCommand]
    private void OpenFullScreenPreview(INavigatorListEntry? navigatorListEntry)
    {
        if (navigatorListEntry == null)
            return;

        if (navigatorListEntry is VideoEntryVM video)
            video.Pause();
        
        var vm = new FullScreenPreviewVM(async entry =>
        {
            var tags = await _fileTagService.GetFileTags(entry.DbId!.Value);
            var frameDataTag = tags.FirstOrDefault(x => x.Tag.Title == "UgoiraFrameData");

            if (frameDataTag == null || string.IsNullOrEmpty(frameDataTag.Value))
                return null;

            var frameData = JsonConvert.DeserializeObject<UgoiraFrameData>(frameDataTag.Value);

            return frameData?.Data.Select(x => new DelayItem(x.Delay, x.File)).ToList();
        },
        entry => _fileNoteService.GetFileNotes(entry.DbId!.Value));

        vm.CurrentEntryNameChanged += OnCurrentEntryNameChanged;
        vm.CloseRequested += OnFullScreenPreviewVMCloseRequested;

        vm.SetCurrentEntry(navigatorListEntry, NavigatorListInternal);

        FullScreenPreviewVM = vm;
        Title = Path.GetFileName(navigatorListEntry.Path);
    }

    private void OpenCreateCollectionWizard()
    {
        var window = new CreateCollectionWizardWindow();
        var vm = new WizardRootVM(window);

        window.DataContext = vm;
        window.Show();
    }

    private void OnCurrentEntryNameChanged(object? _, string value) => Title = value;

    private void OnFullScreenPreviewVMCloseRequested(object? o, EventArgs eventArgs)
    {
        var vm = FullScreenPreviewVM;
        
        if (vm == null)
            return;
        
        vm.CloseRequested -= OnFullScreenPreviewVMCloseRequested;
        vm.CurrentEntryNameChanged -= OnCurrentEntryNameChanged;
        FullScreenPreviewVM = null;
        Title = DefaultTitle;
    }

    private static string CreatePlayList(IList<string> videos, string videoPath)
    {
        var startIndex = videos.IndexOf(videoPath);

        var playlist = new StringBuilder();
        playlist.AppendLine("MPCPLAYLIST");
        for (int i = 0; i < videos.Count; i++)
        {
            var currentVideoIndex = (i + startIndex) % videos.Count;
            var currentVideoPath = videos[currentVideoIndex];
            playlist.AppendLine($"{i + 1},type,0");
            playlist.AppendLine($"{i + 1},filename,{currentVideoPath}");
        }

        playlist.AppendLine();

        var tempPath = Path.GetTempFileName() + ".mpcpl";
        File.WriteAllText(tempPath, playlist.ToString());
        return tempPath;
    }

    [RelayCommand]
    private async Task Shuffle()
    {
        var shuffled = await Task.Run(() =>
        {
            var copy = NavigatorListInternal.ToList();
            return copy.Shuffle().Index().ToDictionary(x => x.Item, x => x.Index);
        });

        lock (NavigatorListInternal)
        {
            NavigatorListInternal.Sort(
                Comparer<INavigatorListEntry>.Create((x, y) => shuffled[x].CompareTo(shuffled[y])));
        }
        LoadPreviews();
    }

    [RelayCommand]
    private void Reverse()
    {
        NavigatorListInternal.Reverse();
        LoadPreviews();
    }

    public void SetStatusError(string error, string message)
    {
        Status = error;
        StatusToolTip = message;
    }

    [RelayCommand]
    private async Task Refresh()
    {
        try
        {
            var tagsToSearch = TagSearchVM.SelectedBindedTags.Select(x => x.Model).ToList();
            var bulkFactor = int.MaxValue;
            var orderMode = ImoutoRebirth.Navigator.Settings.Default.OrderMode.ParseEnumOrDefault<OrderMode>();
            
            var loadingTiming = new Stopwatch();
            loadingTiming.Start();
            await _fileLoadingService.LoadFiles(
                bulkFactor,
                _previewSize,
                TagSearchVM.SelectedCollection.Value,
                tagsToSearch,
                x => TotalCount = x,
                (x, ct) =>
                {
                    IsLoading = false;

                    var entries = orderMode switch
                    {
                        OrderMode.OldFirst => x,
                        OrderMode.Shuffle => x.Shuffle(),
                        OrderMode.NewFirst => x.Reverse(),
                        _ => throw new ArgumentOutOfRangeException()
                    };

                    NavigatorListInternal.AddRange(entries);
                    LoadPreviews();
                },
                () =>
                {
                    TotalCount = 0;
                    NavigatorListInternal.Clear();
                    IsLoading = false;
                },
                () =>
                {
                    _appendNewContentTimer.Stop();
                    IsLoading = true;
                    NavigatorListInternal.Clear();

                },
                () =>
                {
                    IsLoading = false;
                    _appendNewContentTimer.Start();
                });
            loadingTiming.Stop();
            Log.Information("Loading time: {Time}", loadingTiming.ElapsedMilliseconds);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Can't load images from collection");
            SetStatusError("Can't load images from collection", ex.Message);
        }
    }

    private void UpdatePreviews()
    {
        OnPropertyChanged(nameof(SlotSize));

        //Performance ?
        lock (NavigatorListInternal)
        {
            foreach (var imageEntry in NavigatorListInternal)
            {
                imageEntry.UpdatePreview(PreviewSize);
            }
        }

        LoadPreviews();
    }

    [RelayCommand]
    private void LoadPreviews()
    {
        ImageEntry.PreviewLoadingThreadQueue.ClearQueue();
        T.Debounce(
            100,
            () =>
            {
                foreach (var listEntry in _view.VisibleItems)
                {
                    listEntry.Load();
                }
            });
    }

    [RelayCommand]
    private async Task RemoveImage(INavigatorListEntry? selectedItem)
    {
        var dbId = selectedItem?.DbId;

        if (dbId == null)
            return;

        var mySettings = new MetroDialogSettings
        {
            AffirmativeButtonText = "Yes",
            NegativeButtonText = "No",
            ColorScheme = MetroDialogColorScheme.Accented,
            AnimateShow = false,
            AnimateHide = false
        };
        var result = await _view.ShowMessageDialog(
            "Remove Element",
            "Are you sure you want to remove this item from collection?",
            MessageDialogStyle.AffirmativeAndNegative,
            mySettings);

        if (result != MessageDialogResult.Affirmative)
            return;

        try
        {
            await _fileService.RemoveFile(dbId.Value);
        }
        catch (Exception e)
        {
            SetStatusError(e.Message, "Unable to delete");
            await _view.ShowMessageDialog(
                "Remove Element",
                "Unable to remove",
                MessageDialogStyle.Affirmative,
                new MetroDialogSettings());
            return;
        }

        NavigatorListInternal.Remove(selectedItem!);
        Status = "File successfully removed";
        LoadPreviews();
    }

    [RelayCommand]
    private async Task SetAsWallpaper(INavigatorListEntry? entry)
    {
        var path = entry?.Path;
        if (path == null)
            return;

        if (entry is not { Type: ListEntryType.Image or ListEntryType.Png }) 
            return;
        
        WindowsDesktopService.SetWallpaper(path);

        Status = "Wallpaper set";

        if (entry.DbId.HasValue)
            await _fileTagService.SetWasWallpaper(entry.DbId.Value);
    }

    [RelayCommand]
    private void ShowInExplorer(INavigatorListEntry? selectedItem)
    {
        var path = selectedItem?.Path;
        if (path == null)
            return;

        var file = new FileInfo(path);
        
        Process.Start(
             "explorer.exe" , 
            $"""
             /select,"{file.FullName}"
             """);
        Status = "Wallpaper set";
    }

    [RelayCommand]
    private void Copy()
    {
        var lastItems = SelectedEntries.Select(x => x.Path).ToArray();
        if (!lastItems.Any())
        {
            return;
        }

        var fileCollection = new StringCollection();
        fileCollection.AddRange(lastItems);
        Clipboard.SetFileDropList(fileCollection);
    }

    [RelayCommand]
    private void ToggleQuickTagging() => ShowQuickTagging = !ShowQuickTagging;

    public async Task InitializeContextAsync()
    {
        await InitializeAsync();
        await Refresh();
    }

    private async void TagSearchVM_SelectedTagsUpdated(object? sender, EventArgs e) => await Refresh();

    private async void TagSearchVMOnSelectedCollectionChanged(object? sender, EventArgs eventArgs) => await Refresh();

    private void Settings_ShowPreviewOnSelectChanged(object? sender, EventArgs e) => OnPropertyChanged(nameof(ShowPreview));
}
