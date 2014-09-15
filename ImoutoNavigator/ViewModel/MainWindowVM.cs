using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Text;
using System.Windows.Data;
using ImagesDBLibrary.Model;
using ImoutoNavigator.Commands;
using ImoutoNavigator.Model;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Utils;
using System.Threading.Tasks;
using System.Threading;

namespace ImoutoNavigator.ViewModel
{
    internal class MainWindowVM : VMBase
    {
        #region Fields

        private int                                             _previewSide        = 256;
        private readonly MainWindow                             _view;
        private ObservableCollection<ImageEntryVM>              _imageList;
        private ICollectionView                                 _imageListView;
        private IEnumerable<ImageM>                             _dbImages;
        private ObservableCollection<KeyValuePair<TagM, int>>   _tagListCurrent     = new ObservableCollection<KeyValuePair<TagM, int>>();
        private string                                          _searchString;
        private ObservableCollection<TagM>                      _tagListHintBox     = new ObservableCollection<TagM>();
        private CollectionManagerVM                             _collectionManager;
        private bool                                            _isLoading;
        private CancellationTokenSource                         _ctsImageLoading;

        #endregion Fields

        #region Constructors

        public MainWindowVM()
        {
            _collectionManager = new CollectionManagerVM();
            InitializeCommands();

            _view = new MainWindow {DataContext = this};
            _view.Loaded += _view_Loaded;

            _view.Show();
        }

        #endregion Constructors

        #region Properties

        private Size PreviewSize
        {
            get
            {
                return new Size(_previewSide, _previewSide);
            }
        }

        //public ICollectionView ImageList
        //{
        //    get
        //    {
        //        if (_imageListView == null)
        //        {
        //            _imageListView = new ListCollectionView(_imageList);
        //            _imageListView.Filter = o => (o as ImageEntryVM).ImageModel.ContainsTags(TagListCurrent.Select(x => x.Key));
        //        }
        //        _imageListView.Refresh();
        //        return _imageListView;
        //    }
        //}

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

        public ObservableCollection<KeyValuePair<TagM, int>> TagListTop
        {
            get
            {
                return new ObservableCollection<KeyValuePair<TagM, int>>(ImageList.Cast<ImageEntryVM>().Select(x => x.ImageModel).GetTopTags());
            }
        }

        public ObservableCollection<KeyValuePair<TagM, int>> TagListCurrent
        {
            get
            {
                return _tagListCurrent;
            }
            set
            {
                _tagListCurrent = value;
            }
        }

        public ObservableCollection<TagM> TagListHintBox
        {
            get { return _tagListHintBox; }
            set { _tagListHintBox = value; }
        }

        public CollectionManagerVM CollectionManager
        {
            get
            {
                return _collectionManager;
            }
        }

        public string SearchString
        {
            get
            {
                return _searchString;
            }
            set
            {
                //TODO Add find by first letters, add autocomplition
                var newSearchString = value;
                if (newSearchString == "")
                {
                    TagListHintBox.Clear();
                }
                else
                {
                    IEnumerable<TagM> tags = TagM.Tags.Where(x => x.Name.StartsWith(newSearchString));
                    TagListHintBox.Clear();
                    TagListHintBox = new ObservableCollection<TagM>(tags);
                    OnPropertyChanged("TagListHintBox");
                }

                _searchString = value;
            }
        }

        public bool IsLoading
        {
            get { return _isLoading; }
            private set
            {
                _isLoading = value;
                OnPropertyChanged("IsLoading");
            }
        }

        #endregion Properties

        #region Commands

        public ICommand ZoomInCommand { get; set; }
        public ICommand ZoomOutCommand { get; set; }
        public ICommand LoadPreviewsCommand { get; set; }
        public ICommand AddTagToSearch { get; set; }
        public ICommand RemoveTagFromSearch { get; set; }

        private void InitializeCommands()
        {
            ZoomInCommand = new RelayCommand(x =>
                {
                    if (_previewSide > 1024)
                        return;

                    _previewSide = Convert.ToInt32(Math.Floor(_previewSide * 1.1));
                    UpdatePreviews();
                } 
            );
            ZoomOutCommand = new RelayCommand(x =>
                {
                    if (_previewSide < 64)
                        return;
                    _previewSide = Convert.ToInt32(Math.Floor(_previewSide * 0.9));
                    UpdatePreviews();
                }
            );
            LoadPreviewsCommand = new RelayCommand(x => LoadPreviews());

            AddTagToSearch = new RelayCommand(AddSearchTag);
            RemoveTagFromSearch = new RelayCommand(RemoveSearchTag);
        }

        #endregion Commands

        #region Methods

        public void InitializeCollections()
        {
            CollectionM.ActivatedCollectionChanged += (s, e) => Reload();
            if (CollectionM.Collections.Count != 0)
            {
                // TODO save last activated
                CollectionM.Collections.First().Activate();
            }
        }

        private void GetImageList()
        {


            //_imageList = new ObservableCollection<ImageEntryVM>(
            //    Directory.GetFiles(@"T:\art")
            //        .Where(ImageEntry.IsImage)
            //        .Take(10)
            //        .Select(x =>
            //        {
            //            var im = new ImageEntryVM(x, PreviewSize);
            //            return im;
            //        })
            //        .ToList()
            //    );

            CollectionM collection;
            if (CollectionM.Collections.Count == 0)
            {
                var namedType = TagTypeM.Create("FromName");

                collection = CollectionM.Create("MainColleciton");
                //CollectionM.Create("SubColleciton").AddSource(@"C:\Users\Владимир\Downloads\Обои\Test");
                //collection.AddSource(@"C:\Users\Владимир\Downloads\Обои\Test");
                collection.AddSource(@"C:\Users\Владимир\Downloads\Обои\Обои\Замки");
                collection.AddSource(@"C:\Users\oniii-chan\Downloads\temp\source_named");
                collection.AddSource(@"C:\Users\oniii-chan\Downloads\DLS\art");
                collection.AddSource(@"T:\art");
                collection.Activate();

                int i = 0;
                int all = collection.CountImagesWithTags(null);
                var st = DateTime.Now;

                foreach (var image in collection.GetImages())
                {
                    var tags = image
                                .Path
                                .Split(new[] { "\\" }, StringSplitOptions.RemoveEmptyEntries)
                                .Last()
                                .Split(new[]
                                                {
                                                    " ",
                                                    "."
                                                },
                                                StringSplitOptions.RemoveEmptyEntries);

                    var existingTags = tags
                        .Where(x => TagM
                                        .Tags
                                        .Select(y => y.Name.ToLower())
                                        .Contains(x.ToLower()))
                        .Select(x => TagM
                                         .Tags
                                         .First(y => y.Name == x));

                    try
                    {
                        var nonExistingTags = tags
                            .Where(x => !TagM
                                             .Tags
                                             .Select(y => y.Name.ToLower())
                                             .Contains(x.ToLower()))
                            .Select(tag => TagM.Create(tag, namedType));


                        var allTags = existingTags.Concat(nonExistingTags).ToList();
                        image.AddTags(allTags);
                    }
                    catch (Exception e)
                    {
                        //StringBuilder db = new StringBuilder();
                        //foreach (var tag in tags.OrderBy(x=>x))
                        //{
                        //    db.Append(tag);
                        //    db.Append(" " + TagM
                        //                        .Tags
                        //                        .Select(y => y.Name)
                        //                        .Contains(tag));
                        //}
                        //MessageBox.Show(String.Format("{0}\n {1}\n\n {2}\n\n", image.Path, image.Md5, db));
                    }


                    //var allTags = existingTags.Concat(nonExistingTags).ToList();
                    //image.AddTags(allTags);

                    //foreach (var ntag in allTags)
                    //{
                    //    image.AddTag(ntag);
                    //}


                    //var win = new DebugWindow();
                    //win.TextBlock.Text = "Progress: " + ++i + "/" + all + " Time passed: " + (DateTime.Now - st).TotalMilliseconds;
                    //win.Show();
                }
            }
            else
            {
            }

            //collection.Activate();
        }
        
        private void Reload()
        {
            if (CollectionM.ActivatedCollection != null)
            {
                if (_imageList != null)
                {
                    _imageList.Clear();
                }
                else
                {
                    _imageList = new ObservableCollection<ImageEntryVM>();
                }
                IsLoading = true;

                GetImagesFromCollectionAsync(1000);                
            }
        }

        private void UpdatePreviews()
        {
            foreach (var imageEntry in _imageList)
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

        private void AddSearchTag(object param)
        {
            if (param == null)
            {
                return;
            }
            try
            {
                var tag = (KeyValuePair<TagM, int>)param;

                if (TagListCurrent.All(x => x.Key != tag.Key))
                {
                    TagListCurrent.Add(new KeyValuePair<TagM, int>(tag.Key, tag.Value));
                    OnPropertyChanged("ImageList");
                    OnPropertyChanged("TagListTop");
                }
            }
            catch (Exception)
            {
                try
                {
                    var tag = (TagM) param;

                    if (TagListCurrent.All(x => x.Key != tag))
                    {
                        TagListCurrent.Add(new KeyValuePair<TagM, int>(tag, -1));
                        OnPropertyChanged("ImageList");
                        OnPropertyChanged("TagListTop");
                    }

                    SearchString = "";
                }
                catch { }
            }
        }

        private void RemoveSearchTag(object param)
        {
            if (param == null)
            {
                return;
            }
            var tag = (KeyValuePair<TagM, int>)param;

            if (TagListCurrent.Select(x => x.Key).Contains(tag.Key))
            {
                TagListCurrent.Remove(TagListCurrent.First(x => x.Key.Name == tag.Key.Name && x.Key.Type == tag.Key.Type));
                OnPropertyChanged("ImageList");
                OnPropertyChanged("TagListTop");
            }
        }

        private async void GetImagesFromCollectionAsync(int count, int skip = 0, int block = 10)
        {
            // TODO COUNT

            var total = await GetImagesCountFromCollectionAsyncTask() - skip;

            count = (count < total) ? count : total;
            
            if (count == 0)
            {
                return;
            }          
  
            if (_ctsImageLoading != null)
            {
                _ctsImageLoading.Cancel();
            }
            var newCTS = new CancellationTokenSource();
            _ctsImageLoading = newCTS;

            try
            {
                await LoadImages(count, skip, block, _ctsImageLoading.Token);

                LoadPreviews();
            }
            catch (OperationCanceledException)
            { }

            if (_ctsImageLoading == newCTS)
            {
                _ctsImageLoading = null;
            }
        }

        private async Task LoadImages(int count, int skip, int block, CancellationToken ct)
        {
            for (int i = count; i > 0; i -= block)
            {
                var sw = new Stopwatch();
                sw.Start();

                (await GetImagesFromCollectionAsyncTask(block, skip + count - i)).ForEach(x => _imageList.Add(x));

                sw.Stop();
                Debug.WriteLine("Loading {0} elemets, skip {1} elemets in ms: {2}", block, skip + count - i, sw.ElapsedMilliseconds);

                //OnPropertyChanged("ImageList");

                ct.ThrowIfCancellationRequested();

                if (i == count)
                {
                    OnPropertyChanged("ImageList");
                    IsLoading = false;
                }
            }
        }

        private Task<ObservableCollection<ImageEntryVM>> GetImagesFromCollectionAsyncTask(int count, int skip)
        {
            return Task.Run<ObservableCollection<ImageEntryVM>>(() => 
            { 
                return new ObservableCollection<ImageEntryVM>(
                    CollectionM
                        .ActivatedCollection
                        .GetImages(count, 
                                    skip, 
                                    TagListCurrent.Select(x => x.Key).ToList())
                        .Select(x => new ImageEntryVM(x, PreviewSize))
                    );
            });
        }

        private Task<int> GetImagesCountFromCollectionAsyncTask()
        {
            return Task.Run<int>(() =>
            {
                return 
                    CollectionM
                        .ActivatedCollection
                        .CountImagesWithTags(TagListCurrent.Select(x => x.Key).ToList());
            });
        }

        #endregion Methods

        #region Event handlers

        private void _view_Loaded(object sender, RoutedEventArgs e)
        {
            InitializeCollections();
        }

        #endregion Event handlers
    }
}
