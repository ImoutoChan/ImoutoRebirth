using System;
using System.IO;
using System.Windows;
using System.Windows.Media.Imaging;

namespace ImageViewer.Model
{
    class LocalImage
    {        
        public static ResizeType DefaultResizeType = ResizeType.DownscaleToViewPort;

        #region Fields

        protected BitmapImage _image;

        protected string _filePath;
        protected double _zoom = 1;
        
        protected Size _resizedSize;

        #endregion //Fields

        #region Constructors

        /// <summary>
        /// 
        /// </summary>
        /// <param name="imagePath">Path to image.</param>
        public LocalImage(string imagePath)
        {
            _filePath = imagePath;
        }

        #endregion //Constructors

        #region Properties

        public BitmapImage Image
        {
            get
            {
                if (_image == null)
                {
                    try
                    {
                        _image = new BitmapImage(new Uri(_filePath));
                    }
                    catch
                    {
                        throw new Exception("Unavailable image path");
                    }
                }
                return _image;
            }
        }

        public Size ResizedSize
        {
            get
            {
                if (_resizedSize == null)
                {
                    _resizedSize = new Size(Image.PixelWidth, Image.PixelHeight);
                }
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
                        return Model.ImageFormat.JPG;
                        break;
                    case "gif":
                        return Model.ImageFormat.GIF;
                        break;
                    case "bmp":
                        return Model.ImageFormat.BMP;
                        break;
                    case "tiff":
                        return Model.ImageFormat.TIFF;
                        break;
                    case "png":
                        return Model.ImageFormat.PNG;
                        break;
                    default:
                        return Model.ImageFormat.JPG;
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

        public void FreeMemory()
        {
            
            _image = null;
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
        }

        public void ZoomOut()
        {
            _zoom *= 0.9;
        }

        public void ResetZoom()
        {
            _zoom = 1;
        }
        #endregion //Public methods

        #region Methods

        private static Size ResizeImage(Size original, Size viewPort, ResizeType type)
        {
            Size result = new Size();

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
                case ResizeType.NoResize:
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
            Size result = new Size();

            if (original.Height <= viewPort.Height && original.Width <= viewPort.Width)
            {
                return original;
            }
            else
            {                
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

        }

        /// <summary>
        /// Calculate new image size. Downscale or upscale to viewport with aspect saving.
        /// </summary>
        /// <param name="original">Image size.</param>
        /// <param name="viewPort">View port size.</param>
        /// <returns>Calculated size.</returns>
        private static Size FitToViewPort(Size original, Size viewPort)
        {
            Size result = new Size();

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

            Size result = new Size();

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
            Size result = new Size();

            if (original.Width <= viewPort.Width)
            {
                return original;
            }
            else
            {
                double wRatio = original.Width / viewPort.Width; // original.Width = wratio * viewPort.Width

                //Recalculate if vertical scroll visible
                if (original.Height / wRatio > viewPort.Height)
                {
                    viewPort.Width -= SystemParameters.VerticalScrollBarWidth;
                    wRatio = original.Width / viewPort.Width;
                }

                result.Width = viewPort.Width;
                result.Height = original.Height / wRatio;
            }
            return result;
        } 

        #endregion //Methods
    }

    enum ResizeType
    {
        FitToViewPort,
        DownscaleToViewPort,
        FitToViewPortWidth,
        DownscaleToViewPortWidth,
        NoResize,
        Default
    }

    enum ImageFormat
    {
        JPG,
        PNG,
        BMP,
        TIFF,
        GIF
    }
}
