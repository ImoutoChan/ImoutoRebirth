using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using ImoutoRebirth.Common.WebP;
using ImoutoViewer.ViewModel.SettingsModels;
using Size = System.Windows.Size;

namespace ImoutoViewer.Model;

internal class LocalImage
{
    private const ResizeType DefaultResizeType = ResizeType.DownscaleToViewPort;

    #region Static members

    private static double _constZoom;

    public static bool IsZoomFixed { get; set; }

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

    #endregion Static methods

    #endregion Static members

    #region Fields

    private readonly string _filePath;

    private BitmapSource? _image;

    private double _autoResized;
    private double _localZoom;

    #endregion Fields

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

    #endregion Constructors

    #region Properties

    private double LocalZoom
    {
        get => IsZoomFixed ? 1 : _localZoom;
        set => _localZoom = value;
    }

    public BitmapSource? Image
    {
        get
        {
            if (_image == null && !IsError)
            {
                Load();
            }
            return _image;
        }
        private set => _image = value;
    }

    public bool IsError { get; private set; }

    public string? ErrorMessage { get; private set; }

    public Size ResizedSize =>
        Image != null 
            ? new Size(Image.PixelWidth * Zoom, Image.PixelHeight * Zoom) 
            : new Size(0, 0);

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

    public string Name => _filePath.Split('\\')[_filePath.Split('\\').Length - 1];

    public string Path => _filePath;

    public double Zoom => LocalZoom * _autoResized * StaticZoom;

    #endregion Properties

    #region Public methods

    public void FreeMemory()
    {
        _image = null;

        // GC.Collect();
        // GC.WaitForPendingFinalizers();
    }

    public void Resize(Size viewPort, ResizeType resizedType = ResizeType.Default)
    {
        if (Image == null)
            return;

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

    #endregion Public methods

    #region Methods

    private void Load()
    {
        try
        {
            try
            {
                _image = LoadImage(_filePath);
            }
            catch (ArgumentException)
            {
                //Process fail in color reading exception.
                _image = LoadImage(_filePath, BitmapCreateOptions.IgnoreColorProfile);
            }
        }
        catch (Exception e)
        {
            IsError = true;
            ErrorMessage = e.Message;
        }
    }

    private static BitmapSource LoadImage(
        string filePath, 
        BitmapCreateOptions options = BitmapCreateOptions.None)
    {
        var bitmap = GetBitmap(options, filePath);

        var defaultRotation = GetDefaultRotation(bitmap);

        return defaultRotation != RotateFlipType.RotateNoneFlipNone 
            ? Rotate(bitmap, GetTransform(defaultRotation)) 
            : bitmap;
    }

    private static BitmapSource GetBitmap(BitmapCreateOptions options, string filePath)
    {
        var file = new FileInfo(filePath);
        var isWebp = file.FullName.EndsWith(".webp", StringComparison.OrdinalIgnoreCase);

        if (!isWebp)
        {
            var fileUri = new Uri(filePath);
            return BitmapFrame.Create(fileUri, options, BitmapCacheOption.OnDemand);
        }
        
        using var stream = new MemoryStream();
        var decoder = new SimpleDecoder();
        var bytes = File.ReadAllBytes(filePath);
        var bitmap = decoder.DecodeFromBytes(bytes, bytes.Length);
        bitmap.Save(stream, System.Drawing.Imaging.ImageFormat.Png);
        stream.Seek(0, SeekOrigin.Begin);

        return BitmapFrame.Create(stream, options, BitmapCacheOption.OnLoad);
    }

    private static Transform GetTransform(RotateFlipType defaultRotation)
    {
        var result = new TransformGroup();
        switch (defaultRotation)
        {
            case RotateFlipType.RotateNoneFlipNone:
                result.Children.Add(new RotateTransform(0));
                break;
            case RotateFlipType.Rotate90FlipNone:
                result.Children.Add(new RotateTransform(90));
                break;
            case RotateFlipType.Rotate180FlipNone:
                result.Children.Add(new RotateTransform(180));
                break;
            case RotateFlipType.Rotate270FlipNone:
                result.Children.Add(new RotateTransform(270));
                break;
            case RotateFlipType.RotateNoneFlipX:
                result.Children.Add(new RotateTransform(0));
                result.Children.Add(new ScaleTransform(-1, 1, 0, 0));
                break;
            case RotateFlipType.Rotate90FlipX:
                result.Children.Add(new RotateTransform(90));
                result.Children.Add(new ScaleTransform(-1, 1, 0, 0));
                break;
            case RotateFlipType.Rotate180FlipX:
                result.Children.Add(new RotateTransform(180));
                result.Children.Add(new ScaleTransform(-1, 1, 0, 0));
                break;
            case RotateFlipType.Rotate270FlipX:
                result.Children.Add(new RotateTransform(270));
                result.Children.Add(new ScaleTransform(-1, 1, 0, 0));
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(defaultRotation), defaultRotation, null);
        }

        return result;
    }

    private static RotateFlipType GetDefaultRotation(BitmapSource bitmap)
    {
        if (!(bitmap.Metadata is BitmapMetadata bitmapMetadata)
            || !bitmapMetadata.ContainsQuery("System.Photo.Orientation"))
            return RotateFlipType.RotateNoneFlipNone;

        var o = bitmapMetadata.GetQuery("System.Photo.Orientation");

        if (o == null)
            return RotateFlipType.RotateNoneFlipNone;

        switch ((ushort)o)
        {
            case 1:
                return RotateFlipType.RotateNoneFlipNone;
            case 2:
                return RotateFlipType.RotateNoneFlipX;
            case 3:
                return RotateFlipType.Rotate180FlipNone;
            case 4:
                return RotateFlipType.Rotate180FlipX;
            case 5:
                return RotateFlipType.Rotate90FlipX;
            case 6:
                return RotateFlipType.Rotate90FlipNone;
            case 7:
                return RotateFlipType.Rotate270FlipX;
            case 8:
                return RotateFlipType.Rotate270FlipNone;
            default:
                return RotateFlipType.RotateNoneFlipNone;
        }
    }

    private void Rotate(int angle)
    {
        var transformedBitmap = new TransformedBitmap();

        transformedBitmap.BeginInit();
        transformedBitmap.Source = Image;

        var rotateTransform = new RotateTransform(angle);
        transformedBitmap.Transform = rotateTransform;

        transformedBitmap.EndInit();

        Image = transformedBitmap;
    }

    private static BitmapSource Rotate(BitmapSource input, Transform transform)
    {
        var transformedBitmap = new TransformedBitmap();

        transformedBitmap.BeginInit();
        transformedBitmap.Source = input;

        transformedBitmap.Transform = transform;

        transformedBitmap.EndInit();

        return transformedBitmap;
    }

    #endregion Methods

    #region Events

    public event EventHandler? ImageChanged;

    private void OnImageChanged() => ImageChanged?.Invoke(this, EventArgs.Empty);

    #endregion Events
        
    private enum RotateFlipType
    {
        RotateNoneFlipNone = 0,
        Rotate90FlipNone = 1,
        Rotate180FlipNone = 2,
        Rotate270FlipNone = 3,
        RotateNoneFlipX = 4,
        Rotate90FlipX = 5,
        Rotate180FlipX = 6,
        Rotate270FlipX = 7,
    }
}
