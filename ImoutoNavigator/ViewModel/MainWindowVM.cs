using System.Collections.Generic;
using System.Windows.Documents;
using ImagesDBLibrary.Database;
using ImagesDBLibrary.Database.Model;
using ImoutoNavigator.Commands;
using ImoutoNavigator.Model;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace ImoutoNavigator.ViewModel
{
    internal class MainWindowVM : VMBase
    {
        #region Fields

        private int _previewSide = 256;
        private readonly MainWindow _view;
        private ObservableCollection<ImageEntryVM> _imageList;
        private IEnumerable<Image> _dbImages;
        private ObservableCollection<KeyValuePair<Tag, int>> _inSearchTags = new ObservableCollection<KeyValuePair<Tag, int>>();
        private ObservableCollection<KeyValuePair<Tag, int>> _currentTags;
        private string _searchString;
        private ObservableCollection<Tag> _searchTags = new ObservableCollection<Tag>();

        #endregion //Fields

        #region Constructors

        public MainWindowVM()
        {
            GetImageList();
            InitializeCommands();

            _view = new MainWindow {DataContext = this};
            _view.Loaded += _view_Loaded;

            _view.Show();
        }

        #endregion //Constructors

        #region Properties

        private Size PreviewSize
        {
            get
            {
                return new Size(_previewSide, _previewSide);
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

        public ObservableCollection<KeyValuePair<Tag, int>> CurrentTags
        {
            get { return _currentTags; }
            set { _currentTags = value; }
        }

        public ObservableCollection<KeyValuePair<Tag, int>> InSearchTags
        {
            get
            {
                return _inSearchTags;
            }
            set
            {
                _inSearchTags = value;
            }
        }

        public ObservableCollection<Tag> SearchTags
        {
            get { return _searchTags; }
            set { _searchTags = value; }
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
                    SearchTags.Clear();
                }
                else
                {
                    IEnumerable<Tag> tags = ImagesDB.GetTagsStartFrom(newSearchString, 10);
                    SearchTags.Clear();
                    SearchTags = new ObservableCollection<Tag>(tags);
                    OnPropertyChanged("SearchTags");
                }

                _searchString = value;
            }
        }

        #endregion //Properties

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
                    _previewSide = Convert.ToInt32(Math.Floor(_previewSide * 1.1));
                    UpdatePreviews();
                } 
            );
            ZoomOutCommand = new RelayCommand(x =>
                {
                    _previewSide = Convert.ToInt32(Math.Floor(_previewSide * 0.9));
                    UpdatePreviews();
                }
            );
            LoadPreviewsCommand = new RelayCommand(x => LoadPreviews());

            AddTagToSearch = new RelayCommand(SearchTag);
            RemoveTagFromSearch = new RelayCommand(DeSearchTag);
        }

        #endregion //Commands

        #region Methods

        private void GetImageList()
        {
            _imageList = new ObservableCollection<ImageEntryVM>(
                Directory.GetFiles(@"C:\Users\Владимир\Downloads\Обои\Обои\Замки")
                    .Where(ImageEntry.IsImage)
                    .Take(10)
                    .Select(x =>
                    {
                        var im = new ImageEntryVM(x, PreviewSize);
                        return im;
                    })
                    .ToList()
                );
            
            _dbImages = _imageList.Select(x => new Image(x.ImageEntry.FullName));

            ImagesDB.AddTagsToImages(new List<Tag>
                                     {
                                         new Tag("DLS", TagTypes.Copyright),
                                         new Tag("Downloads", TagTypes.Copyright),
                                         new Tag("temp", TagTypes.Copyright),
                                         new Tag("DLS", TagTypes.Copyright),
                                     }, 
                                     _dbImages);


            CurrentTags = new ObservableCollection<KeyValuePair<Tag, int>>(ImagesDB.GetTagsTopFromImages(_dbImages));

        }

        private void Reload()
        {
             GetImageList();
             OnPropertyChanged("ImageList");
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

        private void SearchTag(object param)
        {
            if (param == null)
            {
                return;
            }
            var tag = (KeyValuePair<Tag, int>) param;

            if (InSearchTags.All(x => x.Key != tag.Key))
            {
                InSearchTags.Add(new KeyValuePair<Tag, int>(tag.Key, tag.Value));
                OnPropertyChanged("SearchTags");

                //CurrentTags.Remove(CurrentTags.First(x=>x.Key.Name == tag.Key.Name && x.Key.Type == tag.Key.Type));
                //OnPropertyChanged("CurrentTags");
            }
        }

        private void DeSearchTag(object param)
        {
            if (param == null)
            {
                return;
            }
            var tag = (KeyValuePair<Tag, int>)param;

            if (CurrentTags.All(x => x.Key != tag.Key))
            {
                InSearchTags.Remove(InSearchTags.First(x => x.Key.Name == tag.Key.Name && x.Key.Type == tag.Key.Type));
                OnPropertyChanged("SearchTags");
            }
        }

        #endregion //Methods

        #region Event handlers

        private void _view_Loaded(object sender, RoutedEventArgs e)
        {
            Reload();
        }

        #endregion //Event handlers
    }
}
