using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace ImoutoViewer.Model
{
    class LocalImage
    {
        private const ResizeType DefaultResizeType = ResizeType.DownscaleToViewPort;

        public static bool IsZoomFixed { get; set; }
        private static double _constZoom;
        public static double StaticZoom
        {
            private get
            {
                return IsZoomFixed ? _constZoom : 1;
            }
            set
            {
                _constZoom = value;
            }
        }

        #region Fields

        private readonly string _filePath;

        private BitmapSource _image;

        private double _autoResized;
        private double _localZoom;

        #endregion //Fields

        #region Constructors

        static LocalImage()
        {
            StaticZoom = 1;
            IsZoomFixed = false;
        }

        public LocalImage(string imagePath)
        {
            LocalZoom = 1;
            _filePath = imagePath;
            IsError = false;
        }

        #endregion //Constructors

        #region Properties

        private double LocalZoom
        {
            get
            {
                return IsZoomFixed ? 1 : _localZoom;
            }
            set
            {
                _localZoom = value;
            }
        }

        public BitmapSource Image
        {
            get
            {
                if (_image == null && !IsError)
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
                switch (_filePath.Split('.')[_filePath.Split('.').Length - 1].ToLower())
                {
                    case "jpg":
                    case "jpeg":
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
            get
            {
                return _filePath.Split('\\')[_filePath.Split('\\').Length - 1];
            }
        }

        public string Path
        {
            get
            {
                return _filePath;
            }
        }

        public double Zoom 
        {
            get { return LocalZoom * _autoResized * StaticZoom; }
        }

        #endregion //Properties

        #region Public methods

        private void Load()
        {
            try
            {
                try
                {
                    var bi = new BitmapImage();

                    bi.BeginInit();
                    bi.CacheOption = BitmapCacheOption.OnDemand;
                    bi.UriSource = new Uri(_filePath);
                    bi.EndInit();

                    _image = bi;
                }
                catch (ArgumentException e)
                {
                    //Process fail in color reading exception.
                    var bi = new BitmapImage();

                    bi.BeginInit();
                    bi.CacheOption = BitmapCacheOption.OnDemand;
                    bi.CreateOptions |= BitmapCreateOptions.IgnoreColorProfile;
                    bi.UriSource = new Uri(_filePath);
                    bi.EndInit();

                    _image = bi;
                }
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

        public void ZoomIn()
        {
            if (IsZoomFixed)
            {
                StaticZoom *= 1.1;
            }
            else
            {
                LocalZoom *= 1.1;
            }

            OnImageChanged();
        }

        public void ZoomOut()
        {
            if (IsZoomFixed)
            {
                StaticZoom *= 0.9;
            }
            else
            {
                LocalZoom *= 0.9;
            }

            OnImageChanged();
        }

        public void ResetZoom()
        {
            LocalZoom = 1;
            OnImageChanged();
        }

        public void RotateLeft()
        {
            Rotate(-90);
            OnImageChanged();
        }

        public void RotateRight()
        {
            Rotate(90);
            OnImageChanged();
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
