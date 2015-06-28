using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Media.Imaging;
using Imouto;
using Imouto.Utils;

namespace Imouto.Navigator.Model
{
    class ImageEntry
    {
        public static readonly ThreadQueue PreviewLoadingThreadQueue = new ThreadQueue();
        private const ResizeType DefaultResizeType = ResizeType.FitToViewPort;

        #region Fields

        private readonly string _path;
        private readonly FileInfo _imageFileInfo;
        private BitmapSource _image;
        private Size _viewPort;
        private bool _isLoading; //TODO MAKE COMMON ENUM STATUS NOT FLAGS
        private bool _isLoaded;

        #endregion //Fields

        #region Constructors

        public ImageEntry(string path, Size viewPort = new Size())
        {
            var imageFileInfo = new FileInfo(path);
            if (!imageFileInfo.Exists)
            {
                throw new ArgumentException("File does not exist.");
            }
            if (!ImageFormats.GetSupportedFormatsList().Contains(imageFileInfo.Extension.ToLower()))
            {
                throw new ArgumentException("File format is not supported.");
            }
            
            _path = path;
            _imageFileInfo = imageFileInfo;
            _viewPort = viewPort;

            IsError = false;
        }

        #endregion //Constructors

        #region Properties

        public BitmapSource Image
        {
            get
            {
                if (_image == null && !_isLoading)
                {
                    //LoadAsyns();
                }
                return _image;
            }
        }

        public bool IsError { get; private set; }

        public string ErrorMessage { get; private set; }

        public Size ImageSize
        {
            get
            {
                return new Size(Image.PixelWidth, Image.PixelHeight);
            }
        }

        public Size ViewPort
        {
            get
            {
                return _viewPort;
            }
        }

        public ImageFormat ImageFormat
        {
            get
            {
                switch (_imageFileInfo.Extension.ToLower())
                {
                    case ".jpeg":
                        return ImageFormat.JPEG;
                    case ".jpg":
                        return ImageFormat.JPG;
                    case ".gif":
                        return ImageFormat.GIF;
                    case ".bmp":
                        return ImageFormat.BMP;
                    case ".tiff":
                        return ImageFormat.TIFF;
                    case ".png":
                        return ImageFormat.PNG;
                    default:
                        return ImageFormat.JPG;
                }
            }
        }

        public string Name
        {
            get { return _imageFileInfo.Name; }
        }

        public string FullName
        {
            get
            { return _imageFileInfo.FullName; }
        }

        public bool IsLoading
        {
            get { return _isLoading; }
        }

        #endregion //Properties

        #region Public methods

        public void FreeMemory()
        {
            _image = null;

            GC.Collect();
            GC.WaitForPendingFinalizers();
        }

        public void UpdatePreview(Size viewPort = new Size())
        {
            if (viewPort == _viewPort)
            {
                return;
            }

            _viewPort = viewPort;
            IsError = false;
            _isLoaded = false;
            _image = null;

            OnImageChanged();
        }

        public void DoLoadAsyns()
        {
            if (!_isLoaded)
            {
                LoadAsyns();
            }
        }

        #endregion //Public methods

        #region Methods

        private async void LoadAsyns()
        {
            _isLoading = true;
            OnImageChanged();

            PreviewLoadingThreadQueue.Add(Load);

            //await Task.Run(() => Load());
            //OnImageChanged();

        }

        private void Load()
        {
            try
            {
                if (_isLoaded)
                {
                    return;
                }

                var resizedSize = new Size();

                if (!_viewPort.IsEmpty)
                {
                    if (_viewPort.Width != 0 && _viewPort.Height != 0)
                    {
                        var decoder =
                            BitmapDecoder.Create(new Uri(_path),
                                BitmapCreateOptions.None,
                                BitmapCacheOption.None);
                        var frame = decoder.Frames[0];

                        resizedSize = ResizeImage(new Size(frame.PixelWidth, frame.PixelHeight),
                            new Size(_viewPort.Width, _viewPort.Height),
                            DefaultResizeType);

                        decoder = null;
                    }
                }

                var bi = new BitmapImage();

                bi.BeginInit();

                if (!resizedSize.IsEmpty)
                {
                    if (resizedSize.Width != 0 && resizedSize.Height != 0)
                    {
                        bi.DecodePixelWidth = Convert.ToInt32(resizedSize.Width);
                        bi.DecodePixelHeight = Convert.ToInt32(resizedSize.Height);
                    }
                }

                bi.CacheOption = BitmapCacheOption.OnLoad;
                bi.UriSource = new Uri(_path);
                bi.EndInit();
                bi.Freeze();

                _image = bi;

                if (_isLoading)
                {
                    _isLoading = false;
                    _isLoaded = true;
                }
            }
            catch (Exception e)
            {
                IsError = true;
                ErrorMessage = e.Message;
            }
            finally
            {
                OnImageChanged();
            }
        }

        #endregion //Methods

        #region Events

        public event EventHandler ImageChanged;

        private void OnImageChanged()
        {
            if (ImageChanged != null)
            {
                ImageChanged(this, new EventArgs());
            }
        }

        #endregion //Events

        #region Static methods

        private static Size ResizeImage(Size original, Size viewPort, ResizeType type)
        {
            Size result;

            switch (type)
            {
                case ResizeType.DownscaleToViewPort:
                    result = DownscaleToViewPort(original, viewPort);
                    break;
                case ResizeType.DownscaleToViewPortWidth:
                    result = DownscaleToViewPortWidth(original, viewPort);
                    break;
                case ResizeType.FitToViewPort:
                    result = FitToViewPort(original, viewPort);
                    break;
                case ResizeType.FitToViewPortWidth:
                    result = FitToViewPortWidth(original, viewPort);
                    break;
                default:
                    result = original;
                    break;
            }

            return result;
        }

        /// <summary>
        /// Calculate new image size. Only downscale to viewport with aspect saving.
        /// </summary>
        /// <param name="original">Image size.</param>
        /// <param name="viewPort">View port size.</param>
        /// <returns>Calculated size.</returns>
        private static Size DownscaleToViewPort(Size original, Size viewPort)
        {
            var result = new Size();

            if (original.Height <= viewPort.Height && original.Width <= viewPort.Width)
            {
                return original;
            }

            double hRatio = original.Height / viewPort.Height; // original.Height = hratio * viewPort.Height
            double wRatio = original.Width / viewPort.Width; // original.Width = wratio * viewPort.Width

            if (wRatio > hRatio)
            {
                result.Width = viewPort.Width;
                result.Height = original.Height / wRatio;
            }
            else
            {
                result.Height = viewPort.Height;
                result.Width = original.Width / hRatio;
            }

            return result;
        }

        /// <summary>
        /// Calculate new image size. Downscale or upscale to viewport with aspect saving.
        /// </summary>
        /// <param name="original">Image size.</param>
        /// <param name="viewPort">View port size.</param>
        /// <returns>Calculated size.</returns>
        private static Size FitToViewPort(Size original, Size viewPort)
        {
            var result = new Size();

            double hRatio = original.Height / viewPort.Height; // original.Height = hratio * viewPort.Height
            double wRatio = original.Width / viewPort.Width; // original.Width = wratio * viewPort.Width

            if (wRatio > hRatio)
            {
                result.Width = viewPort.Width;
                result.Height = original.Height / wRatio;
            }
            else
            {
                result.Height = viewPort.Height;
                result.Width = original.Width / hRatio;
            }

            return result;
        }

        private static Size FitToViewPortWidth(Size original, Size viewPort)
        {

            var result = new Size();

            double wRatio = original.Width / viewPort.Width; // original.Width = wratio * viewPort.Width

            //Recalculate if vertical scroll visible
            if (original.Height / wRatio > viewPort.Height)
            {
                viewPort.Width -= SystemParameters.VerticalScrollBarWidth;
                wRatio = original.Width / viewPort.Width;
            }

            result.Width = viewPort.Width;
            result.Height = original.Height / wRatio;

            return result;
        }

        private static Size DownscaleToViewPortWidth(Size original, Size viewPort)
        {
            var result = new Size();

            if (original.Width <= viewPort.Width)
            {
                return original;
            }

            double wRatio = original.Width / viewPort.Width; // original.Width = wratio * viewPort.Width

            //Recalculate if vertical scroll visible
            if (original.Height / wRatio > viewPort.Height)
            {
                viewPort.Width -= SystemParameters.VerticalScrollBarWidth;
                wRatio = original.Width / viewPort.Width;
            }

            result.Width = viewPort.Width;
            result.Height = original.Height / wRatio;
            return result;
        }

        public static bool IsImage(string file)
        {
            var ci = new CultureInfo("en-US");
            const string formats = @".jpg|.png|.jpeg|.bmp|.gif|.tiff";

            return formats.Split('|').Any(item => file.EndsWith(item, true, ci));
        }

        #endregion //Static methods
    }
}
