﻿using Imouto;
using Imouto.Viewer.Behavior;
using Imouto.Viewer.Commands;
using Imouto.Viewer.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Imouto.Viewer.WCF;

namespace Imouto.Viewer.ViewModel
{
    class MainWindowVM : VMBase, IDragable, IDropable
    {
        #region Fields

        private readonly Guid _appGuid = Guid.NewGuid();

        private readonly MainWindow _mainWindowView;
        private LocalImageList _imageList;
        private bool _isSlideshowActive;
        private DispatcherTimer _timer;
        private TagsVM _tagsVM;
        private AddTagVM _addTagVM;
        private CreateTagVM _createTagVM;

        #endregion Fields

        #region Constructors

        public MainWindowVM()
        {
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

            InitializeImageListAsync();

            View.ViewPort.SizeChanged += (sender, args) => UpdateView();
        }

        #endregion Constructors

        #region Properties

        public MainWindow View
        {
            get
            {
                return _mainWindowView;
            }
        }

        public AddTagVM AddTagVM
        {
            get
            {
                return _addTagVM;
            }
        }

        public CreateTagVM CreateTagVM
        {
            get
            {
                return _createTagVM;
            }
        }

        public TagsVM Tags
        {
            get
            {
                return _tagsVM;
            }
        }

        public bool IsSlideshowActive
        {
            get
            {
                return _isSlideshowActive;
            }
            set
            {
                if (_isSlideshowActive)
                {
                    _timer.Stop();
                }
                if (value)
                {
                    _timer = new DispatcherTimer() { Interval = new TimeSpan(0, 0, Settings.SlideshowDelay) };
                    _timer.Tick += (sender, args) => NextImage();
                    _timer.Start();
                }

                _isSlideshowActive = value;
                OnPropertyChanged("IsSlideshowActive");
            }
        }

        public LocalImage CurrentLocalImage
        {
            get
            {
                var bs = _imageList?.CurrentImage.Image;
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

                title += System.Reflection.Assembly.GetExecutingAssembly().GetName().Name.Replace('.', ' ');

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
                if (_imageList == null)
                {
                    return null;
                }

                int i = (int)Math.Log10(_imageList.DirectoriesCount + 1) + 1;
                return String.Format("{0," + i + "} / {1," + i + "}",
                    _imageList.CurrentDirectoryIndex + 1,
                    _imageList.DirectoriesCount);
            }
        }

        public string DirStatusToolTip
        {
            get
            {
                if (_imageList == null)
                {
                    return null;
                }

                return _imageList.CurrentDirectory?.Name;
            }
        }

        public string FileStatus
        {
            get
            {
                if (_imageList == null)
                {
                    return null;
                }

                int i = (int)Math.Log10(_imageList.ImagesCount) + 1;
                return String.Format("{0," + i + "} / {1," + i + "}",
                            _imageList.CurrentImageIndex + 1,
                            _imageList.ImagesCount);
            }
        }

        public string ZoomString
        {
            get
            {
                return CurrentLocalImage != null
                    ? String.Format("{0:N0} %", CurrentLocalImage.Zoom * 100)
                    : "100 %";
            }
        }

        public double Zoom => CurrentLocalImage?.Zoom ?? 1;

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
                return _imageList?.CurrentImage != null && _imageList.CurrentImage.IsError;
            }
        }

        public string ErrorMessage
        {
            get
            {
                return _imageList?.CurrentImage?.ErrorMessage;
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

        public bool IsZoomFixed
        {
            get { return LocalImage.IsZoomFixed; }
        }

        #endregion Properties

        #region Methods

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

        private void Settings_SelectedNotesModeChanged(object sender, EventArgs e)
        {
            Tags.ShowNotes = Settings.ShowNotes;
        }

        private void Settings_SelectedTagsModeChanged(object sender, EventArgs e)
        {
            Tags.ShowTags = Settings.ShowTags;
        }

        private void InitializeImageList(IEnumerable<string> images)
        {
            if (images != null)
            {
                _imageList = new LocalImageList(images);
            }
            else if (Application.Current.Properties["ArbitraryArgName"] != null)
            {
                string[] fname = Application.Current.Properties["ArbitraryArgName"].ToString().Split(new[] { "\n&$&\n" }, StringSplitOptions.RemoveEmptyEntries);
                Application.Current.Properties["ArbitraryArgName"] = null;

                _imageList = new LocalImageList(fname);
            }
#if DEBUG
            else
            {
                _imageList = new LocalImageList(@"D:\!Download\DLS\мясо");
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

        private async void InitializeImageListAsync(IEnumerable<string> images = null)
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

        public ICommand SimpleNextImageCommand { get; private set; }
        public ICommand SimplePrevImageCommand { get; private set; }
        public ICommand NextImageCommand { get; private set; }
        public ICommand PrevImageCommand { get; private set; }
        public ICommand ZoomInCommand { get; private set; }
        public ICommand ZoomOutCommand { get; private set; }
        public ICommand NextFolderCommand { get; private set; }
        public ICommand PrevFolderCommand { get; private set; }
        public ICommand FixZoomCommand { get; private set; }
        public ICommand ToggleSlideshowCommand { get; private set; }

        /// <summary>
        /// Rotation the image. In CommandParameter as string send: 
        /// "right" to rotate image on 90 deg right; 
        /// "left to rotate image on -90 deg left.
        /// </summary>
        public ICommand RotateCommand { get; private set; }
        public ICommand ToggleTagsCommand { get; private set; }
        public ICommand ToggleNotesCommand { get; private set; }

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
            ToggleSlideshowCommand = new RelayCommand(param => ToggleSlideshow(param));
            ToggleTagsCommand = new RelayCommand(param => ToggleTags());
            ToggleNotesCommand = new RelayCommand(param => ToggleNotes());
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

        private void ToggleSlideshow(object param)
        {
            if (param is string)
            {
                if (param as string == "ForcedDisable")
                {
                    IsSlideshowActive = false;
                }
            }
            IsSlideshowActive = !IsSlideshowActive;
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

            OnPropertyChanged("IsZoomFixed");
        }

        #endregion Command handlers

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
            InitializeImageListAsync(new[] { CurrentLocalImage.Path });
        }

        private void Settings_SelectedFoldersSortingChanged(object sender, EventArgs e)
        {
            LocalImageList.FoldersSortMethod = Settings.SelectedFoldersSorting.Method;
            LocalImageList.IsFoldersSortMethodDescending = Settings.IsSelectedFoldersSortingDescending;
            InitializeImageListAsync(new[] { CurrentLocalImage.Path });
        }

        private void Settings_SelectedFilesSortingChanged(object sender, EventArgs e)
        {
            LocalImageList.FilesSortMethod = Settings.SelectedFilesSorting.Method;
            LocalImageList.IsFilesSortMethodDescending = Settings.IsSelectedFilesSortingDescending;
            _imageList.ResortFiles();
            UpdateView();
        }

        private void _imageList_CurrentImageChanged(object sender, EventArgs e)
        {
            _mainWindowView.ScrollViewerObject.IsNeedScrollHome = true;

            _tagsVM.ReloadAsync();
        }

        #endregion Event handlers

        #region IDragable members

        public object Data
        {
            get
            {
                var data = new DataObject(DataFormats.FileDrop, new[] { CurrentLocalImage.Path });
                data.SetData("DragSource", _appGuid);
                return data;
            }
        }

        public DragDropEffects AllowDragDropEffects
        {
            get
            {
                return DragDropEffects.Copy;
            }
        }

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

        public void Drop(object data, object sourceGuid)
        {
            var droppedFiles = (string[])data;
            try
            {
                var typedSourceGuid = (Guid)sourceGuid;
                if (typedSourceGuid == _appGuid)
                {
                    return;
                }
            }
            catch
            { }

            InitializeImageListAsync(droppedFiles);
        }

        #endregion IDropable members
    }
}
