using ImoutoNavigator.Commands;
using ImoutoNavigator.Model;
using ImoutoNavigator.WCF;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Utils;

namespace ImoutoNavigator.ViewModel
{
    internal class MainWindowVM : VMBase
    {
        #region Fields

        private int _previewSize = 256;
        private readonly MainWindow _view;
        private ObservableCollection<ImageEntryVM> _imageList = new ObservableCollection<ImageEntryVM>();
        //private CollectionManagerVM                             _collectionManager;
        private bool _isLoading;
        private CancellationTokenSource _ctsImageLoading;
        private int _totalCount = 0;

        #endregion Fields

        #region Constructors

        public MainWindowVM()
        {
            //GetImageList();
            //_collectionManager = new CollectionManagerVM();
            InitializeCommands();

            TagSearchVM = new TagSearchVM(this);
            TagSearchVM.SelectedTagsUpdated += TagSearchVM_SelectedTagsUpdated;

            ImageList.CollectionChanged += (s, e) => OnPropertyChanged(() => this.LoadedCount);

            _view = new MainWindow { DataContext = this };
            _view.Loaded += _view_Loaded;

            _view.Show();
        }

        #endregion Constructors

        #region Properties

        public Size SlotSize
        {
            get
            {
                return new Size(_previewSize + 30, _previewSize + 30);
            }
        }

        private Size PreviewSize
        {
            get
            {
                return new Size(_previewSize, _previewSize);
            }
        }


        public ObservableCollection<ImageEntryVM> ImageList
        {
            get
            {
                return _imageList;
            }
        }

        public string Title
        {
            get
            {
                return String.Format("Imouto Navigator");
            }
        }

        public TagSearchVM TagSearchVM { get; private set; }

        //public CollectionManagerVM CollectionManager
        //{
        //    get
        //    {
        //        return _collectionManager;
        //    }
        //}

        public bool IsLoading
        {
            get { return _isLoading; }
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
                OnPropertyChanged(ref _totalCount, value, () => this.TotalCount);
            }
        }

        public int LoadedCount
        {
            get
            {
                return ImageList.Count();
            }
        }

        #endregion Properties

        #region Commands

        public ICommand ZoomInCommand { get; set; }
        public ICommand ZoomOutCommand { get; set; }
        public ICommand LoadPreviewsCommand { get; set; }

        private void InitializeCommands()
        {
            ZoomInCommand = new RelayCommand(x =>
                {
                    if (_previewSize > 1024)
                        return;

                    _previewSize = Convert.ToInt32(Math.Floor(_previewSize * 1.1));
                    UpdatePreviews();
                }
            );
            ZoomOutCommand = new RelayCommand(x =>
                {
                    if (_previewSize < 64)
                        return;
                    _previewSize = Convert.ToInt32(Math.Floor(_previewSize * 0.9));
                    UpdatePreviews();
                }
            );
            LoadPreviewsCommand = new RelayCommand(x => LoadPreviews());
        }

        #endregion Commands

        #region Methods

        //public void InitializeCollections()
        //{
        //    CollectionM.ActivatedCollectionChanged += (s, e) => Reload();
        //    if (CollectionM.Collections.Count != 0)
        //    {
        //        // TODO save last activated
        //        CollectionM.Collections.First().Activate();
        //    }
        //}

        private void GetImageList()
        {

            /*
            _imageList = new ObservableCollection<ImageEntryVM>(
                Util
                    .GetDirectories(new DirectoryInfo(@"D:\!ArtCollection\"), true)
                    .SelectMany(x => x.GetFiles().Select(y => y.FullName))
                    .Where(ImageEntry.IsImage)
                    .Select(x =>
                    {
                        var im = new ImageEntryVM(x, PreviewSize);
                        return im;
                    })
                    .ToList()
                );
            */


            //CollectionM collection;
            //if (CollectionM.Collections.Count == 0)
            //{
            //    var namedType = TagTypeM.Create("FromName");

            //    collection = CollectionM.Create("MainColleciton");
            //    //CollectionM.Create("SubColleciton").AddSource(@"C:\Users\Владимир\Downloads\Обои\Test");
            //    //collection.AddSource(@"C:\Users\Владимир\Downloads\Обои\Test");
            //    collection.AddSource(@"C:\Users\Владимир\Downloads\Обои\Обои\Замки");
            //    collection.AddSource(@"C:\Users\oniii-chan\Downloads\temp\source_named");
            //    collection.AddSource(@"C:\Users\oniii-chan\Downloads\DLS\art");
            //    collection.AddSource(@"T:\art");
            //    collection.Activate();

            //    int i = 0;
            //    int all = collection.CountImagesWithTags(null);
            //    var st = DateTime.Now;

            //    foreach (var image in collection.GetImages())
            //    {
            //        var tags = image
            //                    .Path
            //                    .Split(new[] { "\\" }, StringSplitOptions.RemoveEmptyEntries)
            //                    .Last()
            //                    .Split(new[]
            //                                    {
            //                                        " ",
            //                                        "."
            //                                    },
            //                                    StringSplitOptions.RemoveEmptyEntries);

            //        var existingTags = tags
            //            .Where(x => TagM
            //                            .Tags
            //                            .Select(y => y.Name.ToLower())
            //                            .Contains(x.ToLower()))
            //            .Select(x => TagM
            //                             .Tags
            //                             .First(y => y.Name == x));

            //        try
            //        {
            //            var nonExistingTags = tags
            //                .Where(x => !TagM
            //                                 .Tags
            //                                 .Select(y => y.Name.ToLower())
            //                                 .Contains(x.ToLower()))
            //                .Select(tag => TagM.Create(tag, namedType));


            //            var allTags = existingTags.Concat(nonExistingTags).ToList();
            //            image.AddTags(allTags);
            //        }
            //        catch (Exception e)
            //        {
            //            //StringBuilder db = new StringBuilder();
            //            //foreach (var tag in tags.OrderBy(x=>x))
            //            //{
            //            //    db.Append(tag);
            //            //    db.Append(" " + TagM
            //            //                        .Tags
            //            //                        .Select(y => y.Name)
            //            //                        .Contains(tag));
            //            //}
            //            //MessageBox.Show(String.Format("{0}\n {1}\n\n {2}\n\n", image.Path, image.Md5, db));
            //        }


            //        //var allTags = existingTags.Concat(nonExistingTags).ToList();
            //        //image.AddTags(allTags);

            //        //foreach (var ntag in allTags)
            //        //{
            //        //    image.AddTag(ntag);
            //        //}


            //        //var win = new DebugWindow();
            //        //win.TextBlock.Text = "Progress: " + ++i + "/" + all + " Time passed: " + (DateTime.Now - st).TotalMilliseconds;
            //        //win.Show();
            //    }
            //}
            //else
            //{
            //}

            //collection.Activate();
        }

        private void Reload()
        {
            //if (CollectionM.ActivatedCollection != null)
            //{
            //    if (_imageList != null)
            //    {
            //        _imageList.Clear();
            //    }
            //    else
            //    {
            //        _imageList = new ObservableCollection<ImageEntryVM>();
            //    }
            //    IsLoading = true;

            //    GetImagesFromCollectionAsync(1000);                
            //}

            ImageList.Clear();
            GetImagesFromCollectionAsync(100000, 0, 500);
        }

        private void UpdatePreviews()
        {
            //OnPropertyChanged("PreviewSize");
            OnPropertyChanged("SlotSize");
            foreach (var imageEntry in ImageList)
            {
                imageEntry.UpdatePreview(PreviewSize);
            }

            LoadPreviews();
        }

        private void LoadPreviews()
        {
            ImageEntry.PreviewLoadingThreadQueue.ClearQueue();

            foreach (ImageEntryVM imageEntry in _view.VisibleItems)
            {
                imageEntry.Load();
            }
        }

        private static SemaphoreSlim ReloadImagesAsyncSemaphore = new SemaphoreSlim(1, 1);
        private async void GetImagesFromCollectionAsync(int count, int skip = 0, int block = 10)
        {
            // TODO COUNT

            try
            {
                var total = await GetImagesCountFromCollectionAsyncTask() - skip;

                // skip ?
                TotalCount = total + skip;

                count = (count < total) ? count : total;

                if (count == 0)
                {
                    IsLoading = false;
                    return;
                }

                if (_ctsImageLoading != null)
                {
                    _ctsImageLoading.Cancel();
                }

                await ReloadImagesAsyncSemaphore.WaitAsync();

                var newCTS = new CancellationTokenSource();
                _ctsImageLoading = newCTS;

                try
                {
                    await LoadImages(count, skip, block, _ctsImageLoading.Token);

                    LoadPreviews();
                }
                catch (OperationCanceledException)
                { }
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
                Debug.WriteLine("Tags load error: " + ex.Message);
            }
        }

        private async Task LoadImages(int count, int skip, int block, CancellationToken ct)
        {
            for (int i = count; i > 0; i -= block)
            {
                var sw = new Stopwatch();
                sw.Start();

                (await GetImagesFromCollectionAsyncTask(block, skip + count - i)).ForEach(x => ImageList.Add(x));

                sw.Stop();
                Debug.WriteLine("Loading {0} elemets, skip {1} elemets in ms: {2}", block, skip + count - i, sw.ElapsedMilliseconds);

                ct.ThrowIfCancellationRequested();

                if (i == count)
                {
                    IsLoading = false;
                }
            }
        }

        private Task<ObservableCollection<ImageEntryVM>> GetImagesFromCollectionAsyncTask(int count, int skip)
        {
            return Task.Run(() =>
            {
                return new ObservableCollection<ImageEntryVM>(
                    ImoutoService.Use(imoutoService =>
                    {
                        return imoutoService.SearchImage(TagSearchVM.SelectedBindedTags.ToList(), count, skip);
                    })
                    .Select(x => new ImageEntryVM(x, PreviewSize))
                    .SkipExceptions()
                    );
            });
        }

        private Task<int> GetImagesCountFromCollectionAsyncTask()
        {
            return Task.Run(() =>
            {
                return
                    ImoutoService.Use(imoutoService =>
                    {
                        return imoutoService.CountSearchImage(TagSearchVM.SelectedBindedTags.ToList());
                    });
            });
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

        #endregion Event handlers
    }
}
