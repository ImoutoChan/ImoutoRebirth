using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Imouto.Utils.Core;
using ImoutoRebirth.Navigator.Commands;
using ImoutoRebirth.Navigator.Model;
using ImoutoRebirth.Navigator.Services;
using ImoutoRebirth.Navigator.Services.Tags;
using MahApps.Metro.Controls.Dialogs;

namespace ImoutoRebirth.Navigator.ViewModel
{
    class MainWindowVM : VMBase
    {
        #region Fields

        private MainWindow _view;
        private CancellationTokenSource _ctsImageLoading;
        private int _previewSize = 256;
        private int _totalCount;
        private bool _isLoading;
        private string _status;
        private string _statusToolTip;
        private static readonly SemaphoreSlim ReloadImagesAsyncSemaphore = new SemaphoreSlim(1, 1);
        private readonly IFileService _fileService;

        #endregion Fields

        #region Constructors

        public MainWindowVM()
        {
            _fileService = ServiceLocator.GetService<IFileService>();

            InitializeCommands();

            NavigatorList.CollectionChanged += (s, e) => OnPropertyChanged(() => LoadedCount);
        }

        public async Task InitializeAsync()
        {
            await CollectionManager.ReloadCollectionsAsync();

            TagSearchVM = new TagSearchVM(CollectionManager.Collections);
            TagSearchVM.SelectedTagsUpdated += TagSearchVM_SelectedTagsUpdated;
            TagSearchVM.SelectedCollectionCahnged += TagSearchVMOnSelectedCollectionCahnged;

            Settings.ShowPreviewOnSelectChanged += Settings_ShowPreviewOnSelectChanged;

            TagsEdit = new TagsEditVM(this);

            _view = new MainWindow
            {
                DataContext = this
            };
            _view.Loaded += _view_Loaded;
            _view.SelectedItemsChanged += OnViewOnSelectedItemsChanged;
            _view.Show();
        }

        private void OnViewOnSelectedItemsChanged(object sender, EventArgs args)
        {
            OnPropertyChanged(() => SelectedItems);
            TagSearchVM.UpdateCurrentTags(_view.ListBoxElement.SelectedItem as INavigatorListEntry);
            FileInfoVM.UpdateCurrentInfo(
                _view.ListBoxElement.SelectedItem as INavigatorListEntry, 
                NavigatorList.IndexOf(_view.ListBoxElement.SelectedItem as INavigatorListEntry));
        }

        #endregion Constructors

        #region Properties

        public Size SlotSize => new Size(_previewSize + 30, _previewSize + 30);

        private Size PreviewSize => new Size(_previewSize, _previewSize);

        public ObservableCollection<INavigatorListEntry> NavigatorList { get; } = new ObservableCollection<INavigatorListEntry>();

        public string Title => "Imouto Navigator";

        public TagSearchVM TagSearchVM { get; set; }

        public CollectionManagerVm CollectionManager { get; } = new CollectionManagerVm();

        public SettingsVM Settings { get; } = new SettingsVM();

        public TagsEditVM TagsEdit { get; set; }

        public bool IsLoading
        {
            get
            {
                return _isLoading;
            }
            private set
            {
                _isLoading = value;
                OnPropertyChanged("IsLoading");
            }
        }

        public int TotalCount
        {
            get
            {
                return _totalCount;
            }
            set
            {
                OnPropertyChanged(ref _totalCount, value, () => TotalCount);
            }
        }

        public int LoadedCount => NavigatorList.Count();

        public string Status
        {
            get
            {
                return _status;
            }
            set
            {
                OnPropertyChanged(ref _status, value, () => Status);
            }
        }

        public string StatusToolTip
        {
            get
            {
                return _statusToolTip;
            }
            set
            {
                OnPropertyChanged(ref _statusToolTip, value, () => StatusToolTip);
            }
        }

        public bool ShowPreview => Settings.ShowPreviewOnSelect;

        public IEnumerable<INavigatorListEntry> SelectedItems
        {
            get
            {
                return _view.SelectedItems;
            }
        }

        public FileInfoVM FileInfoVM { get; } = new FileInfoVM();

        #endregion Properties

        #region Commands

        public ICommand ShuffleCommand { get; set; }

        public ICommand ZoomInCommand { get; set; }

        public ICommand ZoomOutCommand { get; set; }

        public ICommand LoadPreviewsCommand { get; set; }

        public ICommand RemoveImageCommand { get; set; }

        public ICommand CopyCommand { get; set; }

        #endregion Commands

        #region Methods

        private void InitializeCommands()
        {
            ShuffleCommand = new RelayCommand(x =>
            {
                ShuffleNavigatorList();
            });

            ZoomInCommand = new RelayCommand(x =>
            {
                if (_previewSize > 1024)
                {
                    return;
                }

                _previewSize = Convert.ToInt32(Math.Floor(_previewSize * 1.1));
                UpdatePreviews();
            });
            ZoomOutCommand = new RelayCommand(x =>
            {
                if (_previewSize < 64)
                {
                    return;
                }
                _previewSize = Convert.ToInt32(Math.Floor(_previewSize * 0.9));
                UpdatePreviews();
            });
            LoadPreviewsCommand = new RelayCommand(x => LoadPreviews());
            RemoveImageCommand = new RelayCommand(RemoveImage);

            CopyCommand = new RelayCommand(CopySelected);
        }

        private void ShuffleNavigatorList()
        {
            var randomGenerator = new Random();

            lock (NavigatorList)
            {
                var newCollection = NavigatorList.ToList();
                var count = NavigatorList.Count;

                foreach (var navigatorListEntry in newCollection)
                {
                    NavigatorList.Remove(navigatorListEntry);

                    var newIndex = randomGenerator.Next(0, count - 2);

                    NavigatorList.Insert(newIndex, navigatorListEntry);
                }
            }
        }

        public void SetStatusError(string error, string message)
        {
            Status = error;
            StatusToolTip = message;
        }
        
        private void Reload()
        {

            lock (NavigatorList)
            {
                _ctsImageLoading?.Cancel();

                NavigatorList.Clear();
            }

            GetImagesFromCollectionAsync(0, 200000);
        }

        private void UpdatePreviews()
        {
            OnPropertyChanged("SlotSize");


            //Performance ?
            lock (NavigatorList)
            {
                foreach (var imageEntry in NavigatorList)
                {
                    imageEntry.UpdatePreview(PreviewSize);
                }
            }

            LoadPreviews();
        }

        private void LoadPreviews()
        {
            ImageEntry.PreviewLoadingThreadQueue.ClearQueue();

            foreach (INavigatorListEntry listEntry in _view.VisibleItems)
            {
                listEntry.Load();
            }
        }

        private async void GetImagesFromCollectionAsync(int skip = 0, int block = 10)
        {
            // TODO COUNT

            try
            {
                var sw = new Stopwatch();
                sw.Start();

                var total = await GetImagesCountFromCollectionAsyncTask() - skip;

                sw.Stop();
                Debug.WriteLine($"Counted in {sw.ElapsedMilliseconds}ms.");

                // skip ?
                TotalCount = total + skip;

                var count = total;

                if (count == 0)
                {
                    IsLoading = false;
                    return;
                }

                _ctsImageLoading?.Cancel();

                await ReloadImagesAsyncSemaphore.WaitAsync();

                var newCTS = new CancellationTokenSource();
                _ctsImageLoading = newCTS;

                try
                {
                    sw = new Stopwatch();
                    sw.Start();

                    await LoadImages(count, skip, block, _ctsImageLoading.Token);

                    sw.Stop();
                    Debug.WriteLine($"Loaded in {sw.ElapsedMilliseconds}ms.");

                    LoadPreviews();
                }
                catch (OperationCanceledException)
                {
                }
                finally
                {
                    ReloadImagesAsyncSemaphore.Release();
                }

                if (_ctsImageLoading == newCTS)
                {
                    _ctsImageLoading = null;
                }                
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Can't load images from collection: " + ex.Message);
                SetStatusError("Can't load images from collection", ex.Message);
            }
        }

        private async Task LoadImages(int count, int skip, int block, CancellationToken ct)
        {
            for (var i = count; i > 0; i -= block)
            {
                var sw = new Stopwatch();
                sw.Start();

                var result = await GetImagesFromCollectionAsyncTask(block, skip + count - i);
                
                lock (NavigatorList)
                {
                    ct.ThrowIfCancellationRequested();

                    foreach (var navigatorListEntry in result)
                    {
                        NavigatorList.Add(navigatorListEntry);
                    }
                }

                sw.Stop();
                Debug.WriteLine("Loading {0} elemets, skip {1} elemets in ms: {2}", block, skip + count - i, sw.ElapsedMilliseconds);


                if (i == count)
                {
                    IsLoading = false;
                }
            }
        }

        private async Task<IReadOnlyCollection<INavigatorListEntry>> GetImagesFromCollectionAsyncTask(
            int count, 
            int skip)
        {

            var entries = await Task.Run(async () => await NavigatorListEntries());

            return entries;

            async Task<List<INavigatorListEntry>> NavigatorListEntries()
            {
                var found = await _fileService.SearchFiles(
                    TagSearchVM.SelectedColleciton.Value,
                    TagSearchVM.SelectedBindedTags.Select(x => x.Model).ToList(),
                    count,
                    skip);

                var navigatorListEntries = found
                    .Select(x => EntryVM.GetListEntry(x.Path, PreviewSize, x.Id))
                    .SkipExceptions()
                    .ToList();
                return navigatorListEntries;
            }
        }

        private async Task<int> GetImagesCountFromCollectionAsyncTask()
        {
            return await _fileService.CountFiles(
                TagSearchVM.SelectedColleciton.Value,
                TagSearchVM.SelectedBindedTags.Select(x => x.Model).ToList());
        }

        private async void RemoveImage(object o)
        {
            var selectedItem = o as INavigatorListEntry;

            if (!selectedItem.DbId.HasValue)
            {
                return;
            }

            var mySettings = new MetroDialogSettings
            {
                AffirmativeButtonText = "Yes",
                NegativeButtonText = "No",
                ColorScheme = MetroDialogColorScheme.Accented
            };
            var result = await _view.ShowMessageDialog(
                "Remove Element", 
                $"Are you sure you want to remove this element from database?", 
                MessageDialogStyle.AffirmativeAndNegative, 
                mySettings);

            if (result != MessageDialogResult.Affirmative) 
                return;


            await _fileService.RemoveFile(selectedItem.DbId.Value);

            // NavigatorList.Remove(selectedItem);

            await _view.ShowMessageDialog(
                "Remove Element",
                "Element successfully removed.",
                MessageDialogStyle.Affirmative,
                new MetroDialogSettings
                {
                    ColorScheme = MetroDialogColorScheme.Accented
                });
        }
        
        private void CopySelected(object o)
        {
            var lastItems = SelectedItems.Select(x => x.Path).ToArray();
            if (!lastItems.Any())
            {
                return;
            }

            var fileCollection = new StringCollection();
            fileCollection.AddRange(lastItems);
            Clipboard.SetFileDropList(fileCollection);
        }

        #endregion Methods

        #region Event handlers

        private void _view_Loaded(object sender, RoutedEventArgs e)
        {
            //InitializeCollections();
            Reload();
        }

        private void TagSearchVM_SelectedTagsUpdated(object sender, EventArgs e)
        {
            Reload();
        }

        private void TagSearchVMOnSelectedCollectionCahnged(object sender, EventArgs eventArgs)
        {
            Reload();
        }

        private void Settings_ShowPreviewOnSelectChanged(object sender, EventArgs e)
        {
            OnPropertyChanged(() => ShowPreview);
        }

        #endregion Event handlers
    }
}
