using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace ImoutoNavigator.Model
{
    class ImageEntry
    {
        #region Fields

        private readonly string _path;
        private readonly FileInfo _imageFileInfo;
        private BitmapSource _image;
        private Size _hardResize;

        #endregion //Fields

        #region Constructors

        public ImageEntry(string path, Size hardResize = new Size())
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

            _hardResize = hardResize;

            IsError = true;
        }

        #endregion //Constructors

        #region Properties

        public BitmapSource Image
        {
            get
            {
                if (_image == null)
                {
                    Load();
                }
                return _image;
            }
            private set
            {
                _image = value;
            }
        }

        public bool IsError { get; private set; }

        public string ErrorMessage { get; private set; }

        public Size ResizedSize
        {
            get
            {
                return new Size(Image.PixelWidth * Zoom, Image.PixelHeight * Zoom);
            }
        }

        public ImageFormat ImageFormat
        {
            get
            {
                switch (_imageFileInfo.Extension.ToLower())
                {
                    case "jpeg":
                        return ImageFormat.JPEG;
                    case "jpg":
                        return ImageFormat.JPG;
                    case "gif":
                        return ImageFormat.GIF;
                    case "bmp":
                        return ImageFormat.BMP;
                    case "tiff":
                        return ImageFormat.TIFF;
                    case "png":
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

        //public double Zoom
        //{
        //    get { return LocalZoom * _autoResized * StaticZoom; }
        //}

        #endregion //Properties

        #region Public methods

        private void Load()
        {
            try
            {
                var bi = new BitmapImage();

                bi.BeginInit();

                if (!_hardResize.IsEmpty)
                {
                    if (_hardResize.Width != 0)
                    {
                        bi.DecodePixelWidth = Convert.ToInt32(_hardResize.Width);
                    }
                    if (_hardResize.Height != 0)
                    {
                        bi.DecodePixelHeight = Convert.ToInt32(_hardResize.Height);
                    }
                }

                bi.CacheOption = BitmapCacheOption.OnLoad;
                bi.UriSource = new Uri(_path);
                bi.EndInit();

                _image = bi;
            }
            catch (Exception e)
            {
                IsError = true;
                ErrorMessage = e.Message;
            }
        }

        public void FreeMemory()
        {
            _image = null;

            GC.Collect();
            GC.WaitForPendingFinalizers();
        }

        public void Resize(Size viewPort, ResizeType resizedType = ResizeType.Default)
        {
            if (IsZoomFixed)
            {
                _autoResized = 1;
            }
            else
            {
                if (resizedType == ResizeType.Default)
                {
                    resizedType = DefaultResizeType;
                }

                 var resizedSize = ResizeImage(new Size(Image.PixelWidth, Image.PixelHeight),
                                           new Size(viewPort.Width, viewPort.Height),
                                           resizedType);

                 _autoResized = resizedSize.Width / Image.PixelWidth;
            }
        }

        #endregion //Public methods

        #region Methods

        private void Rotate(int angle)
        {
            var trasformedBitmap = new TransformedBitmap();

            trasformedBitmap.BeginInit();
            trasformedBitmap.Source = Image;

            var rotateTransform = new RotateTransform(angle);
            trasformedBitmap.Transform = rotateTransform;

            trasformedBitmap.EndInit();

            Image = trasformedBitmap;
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

        public static LocalImage GetEmptyImage()
        {
            return new LocalImage(@"pack://application:,,,/Resources/img/nothing.png");
        }

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

        #endregion //Static methods
    }
}
