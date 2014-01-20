using ImageViewer.Model;
using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Linq;
using System.Globalization;
using System.Windows.Media.Imaging;
using System;
using ImageViewer.Commands;
using System.Windows.Input;

namespace ImageViewer.ViewModel
{
    class MainWindowVM : VMBase
    {
        #region Fields

        private MainWindow _mainWindowView;
        private LocalImageList _imageList;
        private ResizeType _currentResizeType = ResizeType.Default;
        private Visibility _verticalScrollVisibility;

        #endregion //Fields
        
        #region Constructors

        public MainWindowVM()
        {
            IsSimpleWheelNavigationEnable = true;

            _mainWindowView = new MainWindow();
            _mainWindowView.DataContext = this;
            _mainWindowView.SizeChanged += _mainWindowView_SizeChanged;

            InitializeImageList();
            InitializeCommands();

            _mainWindowView.Show();
        }

        #endregion //Constructors

        #region Properties

        private LocalImage CurrentLocalImage
        {
            get
            {
                _imageList.CurrentImage.Resize(_mainWindowView.Client.RenderSize, _currentResizeType);
                return _imageList.CurrentImage;
            }
        }

        public string Title
        {
            get
            {
                return String.Format("{1} / {2} | File: {0}", CurrentLocalImage.Name, _imageList.CurrentImageIndex + 1, _imageList.Count);
            }
        }

        public BitmapSource Image
        {
            get
            {
                if (CurrentLocalImage.ImageFormat == ImageFormat.GIF)
                {
                    return null;
                }
                else
                {
                    return CurrentLocalImage.Image;
                }
            }
        }

        public double ViewportHeight
        {
            get
            {
                return CurrentLocalImage.ResizedSize.Height;
            }
        }

        public double ViewportWidth
        {
            get
            {
                return CurrentLocalImage.ResizedSize.Width;
            }
        }

        public bool IsSimpleWheelNavigationEnable { get; set; }

        #endregion //Properties

        #region Methods

        private void InitializeImageList()
        {
            if (Application.Current.Properties["ArbitraryArgName"] != null)
            {
                string fname = Application.Current.Properties["ArbitraryArgName"].ToString();
                //MessageBox.Show(fname);
                FileInfo fi = new FileInfo(fname);
                DirectoryInfo di = fi.Directory;

                var files =
                    from file in Directory.GetFiles(di.FullName, "*.*")
                    where IsImage(file)
                    select file;

                _imageList = new LocalImageList(files.ToArray(), fname);
            }
            #if DEBUG
            else
            {
                var files =
                    from file in Directory.GetFiles((new DirectoryInfo(@"c:\Users\oniii-chan\Downloads\DLS\art\loli\")).FullName, "*.*")
                    where IsImage(file)
                    select file;

                _imageList = new LocalImageList(files.ToArray());
            }
            #else
            else
            {
                _imageList = null; 
                //TODO new LocalImageList(); - empty imageList
            }            
            #endif
        }

        private bool IsImage(string file)
        {
            CultureInfo ci = new CultureInfo("en-US");
            string formats = @".jpg|.png|.jpeg|.bmp|.gif|.tiff";
            bool result = false;

            foreach (var item in formats.Split('|'))
            {
                result = result || file.EndsWith(item, true, ci);
                if (result) break;
            }

            return result;
        }

        private void UpdateView()
        {
            OnPropertyChanged("Title");
            OnPropertyChanged("ViewportHeight");
            OnPropertyChanged("ViewportWidth");
            OnPropertyChanged("Image");
        }

        #endregion //Methods

        #region Commands

        public ICommand SimpleNextImageCommand { get; private set; }
        public ICommand SimplePrevImageCommand { get; private set; }
        public ICommand NextImageCommand { get; private set; }
        public ICommand PrevImageCommand { get; private set; }
        public ICommand ZoomInCommand { get; private set; }
        public ICommand ZoomOutCommand { get; private set; }

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

            RotateCommand = new RelayCommand(param => Rotate(param));
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
                default:
                    break;
            }
            UpdateView();
        }

        #endregion //Command handlers

        #region Event handlers

        private void _mainWindowView_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            UpdateView();
        }

        #endregion //Event handlers
    }
}
