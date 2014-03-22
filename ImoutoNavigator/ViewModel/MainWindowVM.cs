using System.Threading.Tasks;
using ImoutoNavigator.Commands;
using ImoutoNavigator.Model;
using ImoutoNavigator.Utils;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace ImoutoNavigator.ViewModel
{
    class MainWindowVM : VMBase
    {
        private static ThreadQueue _previewUpdateThreadQueue = new ThreadQueue();

        #region Fields

        private int _previewSide = 256;
        private readonly MainWindow _view;
        private ObservableCollection<ImageEntryVM> _imageList;

        #endregion //Fields

        #region Constructors

        public MainWindowVM()
        {
            InitializeCommands();

            _view = new MainWindow { DataContext = this };
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
            get { return _imageList; }
        }

        public string Title
        {
            get { return String.Format("Imouto Navigator"); }
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
                Directory.GetFiles(@"c:\Users\oniii-chan\Downloads\DLS\art\")
                    .Where(ImageEntry.IsImage)
                    .Select(x =>
                    {
                        var im = new ImageEntryVM(x, PreviewSize);
                        im.ImageEntryChanged += im_ImageChanged;
                        return im;
                    })
                    .ToList()
                );
        }

        private void Reload()
        {
             GetImageList();
             OnPropertyChanged("ImageList");
        }

        private void UpdatePreviews()
        {

            //Parallel.ForEach(_imageList, imageEntry => imageEntry.UpdatePreview(PreviewSize));
            //_previewUpdateThreadQueue.ClearQueue();
            foreach (var imageEntry in _imageList)
            {
                imageEntry.UpdatePreview(PreviewSize);
                //_previewUpdateThreadQueue.Add(() => imageEntry.UpdatePreview(PreviewSize));
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

        private void im_ImageChanged(object sender, EventArgs e)
        {
        }

        #endregion //Event handlers
    }
}
