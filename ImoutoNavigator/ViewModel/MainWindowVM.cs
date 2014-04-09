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

        public ObservableCollection<Tag> TopTags
        {
            get
            {
                var res = new ObservableCollection<Tag>(ImagesDB.GetTagsTopFromImages(_dbImages).Select(x => x.Key));
                return res;
            }
        }


        #endregion //Properties

        #region Commands

        public ICommand ZoomInCommand { get; set; }
        public ICommand ZoomOutCommand { get; set; }
        public ICommand LoadPreviewsCommand { get; set; }

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

        }

        #endregion //Commands

        #region Methods

        private void GetImageList()
        {
            _imageList = new ObservableCollection<ImageEntryVM>(
                Directory.GetFiles(@"C:\Users\oniii-chan\Downloads\DLS\")
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

        #endregion //Methods

        #region Event handlers

        private void _view_Loaded(object sender, RoutedEventArgs e)
        {
            Reload();
        }

        #endregion //Event handlers
    }
}
