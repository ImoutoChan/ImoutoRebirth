using ImoutoViewer.Behavior;
using ImoutoViewer.Commands;
using ImoutoViewer.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace ImoutoViewer.ViewModel
{
    class MainWindowVM : VMBase, IDragable, IDropable
    {
        #region Fields

        private readonly MainWindow _mainWindowView;
        private LocalImageList _imageList;
        private readonly BackgroundWorker _initBackgroundWorker;

        #endregion //Fields
        
        #region Constructors

        public MainWindowVM()
        {
            OpenWith = new OpenWithVM();
            IsSimpleWheelNavigationEnable = true;            

            _mainWindowView = new MainWindow {DataContext = this};
            _mainWindowView.SizeChanged += _mainWindowView_SizeChanged;

            InitializeCommands();
            InitializeSettings();

            _mainWindowView.Show();

            _initBackgroundWorker = new BackgroundWorker();
            _initBackgroundWorker.RunWorkerCompleted += _backgroundWorker_RunWorkerCompleted;
            _initBackgroundWorker.DoWork += _backgroundWorker_DoWork;
            _initBackgroundWorker.RunWorkerAsync();
        }

        #endregion //Constructors

        #region Properties

        private LocalImage CurrentLocalImage
        {
            get
            {
                var bs = _imageList.CurrentImage.Image;
                if (IsError)
                {
                    return null;
                }

                _imageList.CurrentImage.Resize(_mainWindowView.Client.RenderSize, Settings.SelectedResizeType.Type);
                return _imageList.CurrentImage;
            }
        }

        public string Title
        {
            get
            {
                string title = "";

                if (CurrentLocalImage != null && !_imageList.IsEmpty)
                {
                    title += CurrentLocalImage.Name + " - " ;
                }

                title += System.Reflection.Assembly.GetExecutingAssembly().GetName().Name;

                return title;
            }
        }

        public string Status
        {
            get
            {
                if (IsError)
                {
                    return "Image loading error";
                }

                if (IsLoading)
                {
                    return "Loading...";
                }

                return "Ready";
            }
        }

        public string DirStatus
        {
            get
            {
                int i = (int)Math.Log10(_imageList.DirectoriesCount) + 1;
                return String.Format("{0," + i + "} / {1," + i + "}",
                    _imageList.CurrentDirectoryIndex + 1,
                    _imageList.DirectoriesCount);
            }
        }

        public string DirStatusToolTip
        {
            get
            {
                return _imageList.CurrentDirectory.Name;
            }
        }

        public string FileStatus
        {
            get
            {
                int i = (int)Math.Log10(_imageList.ImagesCount) + 1;
                return String.Format("{0," + i + "} / {1," + i + "}",
                            _imageList.CurrentImageIndex + 1,
                            _imageList.ImagesCount);
            }
        }

        public string Zoom
        {
            get
            {
                return CurrentLocalImage != null
                    ? String.Format("{0:N0} %", CurrentLocalImage.Zoom * 100) 
                    : "100 %";
            }
        }

        public BitmapSource Image
        {
            get 
            {
                return CurrentLocalImage == null || CurrentLocalImage.ImageFormat == ImageFormat.GIF
                    ? null
                    : CurrentLocalImage.Image;
            }
        }

        public BitmapSource AnimutedImage
        {
            get 
            {
                return CurrentLocalImage != null && CurrentLocalImage.ImageFormat == ImageFormat.GIF
                    ? CurrentLocalImage.Image
                    : null;
            }
        }

        public bool IsAnimuted
        {
            get
            {
                return CurrentLocalImage != null && CurrentLocalImage.ImageFormat == ImageFormat.GIF;
            }
        }

        public bool IsLoading { get; private set; }

        public double ViewportHeight
        {
            get
            {
                return CurrentLocalImage == null ? 0 : CurrentLocalImage.ResizedSize.Height;
            }
        }

        public double ViewportWidth
        {
            get
            {
                return CurrentLocalImage == null ? 0 : CurrentLocalImage.ResizedSize.Width;
            }
        }

        public bool IsSimpleWheelNavigationEnable { private get; set; }

        public SettingsVM Settings { get; private set; }

        public OpenWithVM OpenWith { get; private set; }

        public bool IsError
        {
            get
            {
                return _imageList.CurrentImage.IsError;
            }
        }

        public string ErrorMessage
        {
            get
            {
                return _imageList.CurrentImage.ErrorMessage;
            }
        }

        public string ImagePath
        {
            get
            {
                return CurrentLocalImage != null
                    ? CurrentLocalImage.Path
                    : "";
            }
        }

        #endregion //Properties

        #region Methods

        private void InitializeSettings()
        {
            Settings = new SettingsVM();
            Settings.SelectedResizeTypeChanged += _settings_SelectedResizeTypeChanged;
            Settings.SelectedDirectorySearchTypeChanged += _settings_SelectedDirectorySearchTypeChanged;
            Settings.SelectedFilesSortingChanged += Settings_SelectedFilesSortingChanged;
            Settings.SelectedFoldersSortingChanged += Settings_SelectedFoldersSortingChanged;

            LocalImageList.FilesGettingMethods = Settings.DirectorySearchFlags;
            LocalImageList.FilesSortMethod = Settings.SelectedFilesSorting.Method;
            LocalImageList.IsFilesSortMethodDescending = Settings.IsSelectedFilesSortingDescending;
            LocalImageList.FoldersSortMethod = Settings.SelectedFoldersSorting.Method;
            LocalImageList.IsFoldersSortMethodDescending = Settings.IsSelectedFoldersSortingDescending;
        }

        private void InitializeImageList(IEnumerable<string> images = null)
        {
            if (images != null)
            {
                _imageList = new LocalImageList(images);
            }
            else if (Application.Current.Properties["ArbitraryArgName"] != null)
            {
                string[] fname = Application.Current.Properties["ArbitraryArgName"].ToString().Split(new []{' '}, StringSplitOptions.RemoveEmptyEntries);
                Application.Current.Properties["ArbitraryArgName"] = null;

                _imageList = new LocalImageList(fname);
            }
            #if DEBUG
            else
            {
                _imageList = new LocalImageList(@"c:\Users\oniii-chan\Downloads\DLS\art\loli\715e2f290f6c236fdd6426d83ab9a9e0.jpg");
            }
            #else
            else
            {
                _imageList = new LocalImageList();
            }            
            #endif

            _imageList.CurrentImageChanged += _imageList_CurrentImageChanged;
        }

        private void UpdateView()
        {
            try
            {
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
                OnPropertyChanged("Zoom");
                OnPropertyChanged("ImagePath");

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
            catch
            {
                NextImage();
            }
        }

        #endregion //Methods

        #region Commands

        public ICommand SimpleNextImageCommand { get; private set; }
        public ICommand SimplePrevImageCommand { get; private set; }
        public ICommand NextImageCommand { get; private set; }
        public ICommand PrevImageCommand { get; private set; }
        public ICommand ZoomInCommand { get; private set; }
        public ICommand ZoomOutCommand { get; private set; }
        public ICommand NextFolderCommand { get; private set; }
        public ICommand PrevFolderCommand { get; private set; }
        public ICommand FixZoomCommand { get; private set; }

        /// <summary>
        /// Rotation the image. In CommandParameter as string send: 
        /// "right" to rotate image on 90 deg right; 
        /// "left to rotate image on -90 deg left.
        /// </summary>
        public ICommand RotateCommand { get; private set; }

        private void InitializeCommands()
        {
            SimpleNextImageCommand = new RelayCommand(param => 
                {
                    if (IsSimpleWheelNavigationEnable)
                    {
                        NextImage();
                    }
                });

            SimplePrevImageCommand = new RelayCommand(param =>
                {
                    if (IsSimpleWheelNavigationEnable)
                    {
                        PrevImage();
                    }
                });

            NextImageCommand = new RelayCommand(param => NextImage());
            PrevImageCommand = new RelayCommand(param => PrevImage());

            ZoomInCommand = new RelayCommand(param => ZoomIn());
            ZoomOutCommand = new RelayCommand(param => ZoomOut());

            RotateCommand = new RelayCommand(Rotate);

            NextFolderCommand = new RelayCommand(param => NextFolder());
            PrevFolderCommand = new RelayCommand(param => PrevFolder());

            FixZoomCommand = new RelayCommand(param => FixZoom());
        }

        #endregion //Commands

        #region Command handlers

        private void NextImage()
        {            
            _imageList.Next();
            UpdateView();
        }

        private void PrevImage()
        {
            _imageList.Previous();
            UpdateView();
        }

        private void ZoomIn()
        {
            CurrentLocalImage.ZoomIn();
            UpdateView();
        }

        private void ZoomOut()
        {
            CurrentLocalImage.ZoomOut();
            UpdateView();
        }

        private void Rotate(object param)
        {
            switch (param as string)
            {
                case "left":
                    CurrentLocalImage.RotateLeft();
                    break;
                case "right":
                    CurrentLocalImage.RotateRight();
                    break;
            }
            UpdateView();
        }

        private void NextFolder()
        {
            _imageList.NextFolder();
            UpdateView();
        }

        private void PrevFolder()
        {
            _imageList.PrevFolder();
            UpdateView();
        }

        private void FixZoom()
        {
            LocalImage.StaticZoom = !LocalImage.IsZoomFixed ? CurrentLocalImage.Zoom : 1;
            LocalImage.IsZoomFixed = !LocalImage.IsZoomFixed;
        }

        #endregion //Command handlers

        #region Event handlers

        private void _mainWindowView_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            UpdateView();
        }

        private void _settings_SelectedResizeTypeChanged(object sender, EventArgs e)
        {
            UpdateView();
        }

        private void _settings_SelectedDirectorySearchTypeChanged(object sender, EventArgs e)
        {
            LocalImageList.FilesGettingMethods = Settings.DirectorySearchFlags;
            _initBackgroundWorker.RunWorkerAsync(new[] { CurrentLocalImage.Path });
        }

        private void Settings_SelectedFoldersSortingChanged(object sender, EventArgs e)
        {
            LocalImageList.FoldersSortMethod = Settings.SelectedFoldersSorting.Method;
            LocalImageList.IsFoldersSortMethodDescending = Settings.IsSelectedFoldersSortingDescending;
            _initBackgroundWorker.RunWorkerAsync(new[] { CurrentLocalImage.Path });
        }

        private void Settings_SelectedFilesSortingChanged(object sender, EventArgs e)
        {
            LocalImageList.FilesSortMethod = Settings.SelectedFilesSorting.Method;
            LocalImageList.IsFilesSortMethodDescending = Settings.IsSelectedFilesSortingDescending;
            _imageList.ResortFiles();
            UpdateView();
        }

        private void _backgroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            IsLoading = false;
            UpdateView();
        }

        private void _backgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            IsLoading = true;
            OnPropertyChanged("Status");
            OnPropertyChanged("IsLoading");

            if ((e.Argument as string[]) != null)
            {
                InitializeImageList(e.Argument as string[]);
            }
            else
            {
                InitializeImageList();
            }
        }

        private void _imageList_CurrentImageChanged(object sender, EventArgs e)
        {
            _mainWindowView.ScrollViewerObject.IsNeedScrollHome = true;
        }

        #endregion //Event handlers

        #region IDragable members

        public object Data 
        {
            get
            {
                return new DataObject(DataFormats.FileDrop, new[] { CurrentLocalImage.Path });
            }
        }

        public DragDropEffects AllowDragDropEffects
        {
            get
            {
                return DragDropEffects.Copy;
            }
        }

        #endregion //IDragable members

        #region IDropable members

        public List<string> AllowDataTypes
        {
            get
            {
                var list = new List<string> { DataFormats.FileDrop };

                return list;
            }
        }

        public void Drop(object data)
        {
            var droppedFiles = (string[])data;

            _initBackgroundWorker.RunWorkerAsync(droppedFiles);
        }

        #endregion //IDropable members
    }
}
