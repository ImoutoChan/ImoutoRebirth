using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace ImoutoViewer.Model
{
    class LocalImage
    {        
        public static ResizeType DefaultResizeType = ResizeType.DownscaleToViewPort;

        #region Fields

        private readonly string _filePath;

        private BitmapSource _image;
        private double _zoom;
        private Size _resizedSize;
        
        #endregion //Fields

        #region Constructors

        /// <summary>
        /// 
        /// </summary>
        /// <param name="imagePath">Path to image.</param>
        public LocalImage(string imagePath)
        {
            _zoom = 1;
            _filePath = imagePath;
            IsError = false;
        }

        #endregion //Constructors

        #region Properties

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
                return new Size(_resizedSize.Width * _zoom, _resizedSize.Height * _zoom);
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
                        break;
                    case "gif":
                        return ImageFormat.GIF;
                        break;
                    case "bmp":
                        return ImageFormat.BMP;
                        break;
                    case "tiff":
                        return ImageFormat.TIFF;
                        break;
                    case "png":
                        return ImageFormat.PNG;
                        break;
                    default:
                        return ImageFormat.JPG;
                        break;
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

        #endregion //Properties

        #region Public methods

        private void Load()
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
            catch (Exception e)
            {
                IsError = true;
                ErrorMessage = e.Message;
            }
        }

        public void FreeMemory()
        {
            _image = null;
#if DEBUG
            var s = new Stopwatch();
            s.Start();
#endif
            GC.Collect();
            GC.WaitForPendingFinalizers();

#if DEBUG
            s.Stop();
            DebugClass.Add(s.ElapsedMilliseconds);
#endif

        }
        
        public void Resize(Size viewPort, ResizeType resizedType = ResizeType.Default)
        {
            if (resizedType == ResizeType.Default)
            {
                resizedType = DefaultResizeType;
            }

            _resizedSize = ResizeImage(new Size(Image.PixelWidth, Image.PixelHeight),
                                       new Size(viewPort.Width, viewPort.Height),
                                       resizedType);
        }

        public void ZoomIn()
        {
            _zoom *= 1.1;
            OnImageChanged();
        }

        public void ZoomOut()
        {
            _zoom *= 0.9;
            OnImageChanged();
        }

        public void ResetZoom()
        {
            _zoom = 1; 
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

        #region Static members

        public static LocalImage GetEmptyImage()
        {
            return new LocalImage(@"pack://application:,,,/Resources/img/nothing.png");
        }

        #endregion //Static members
    }
}
