using System.Globalization;
using System.IO;
using System.Windows;
using System.Windows.Media.Imaging;
using Imouto;
using ImoutoRebirth.Navigator.Utils;

namespace ImoutoRebirth.Navigator.Model;

internal class ImageEntry
{
    public static readonly AsyncThreadQueue PreviewLoadingThreadQueue = new AsyncThreadQueue();
    private const ResizeType DefaultResizeType = ResizeType.FitToViewPort;
    private Size? _frameSize;
    private readonly string _path;
    private readonly FileInfo _imageFileInfo;
    private BitmapSource? _image;
    private Size _viewPort;
    private bool _isLoading; //TODO MAKE COMMON ENUM STATUS NOT FLAGS
    private bool _isLoaded;

    public ImageEntry(string path, Size viewPort = new Size())
    {
        var imageFileInfo = new FileInfo(path);

        _path = path;
        _imageFileInfo = imageFileInfo;
        _viewPort = viewPort;

        IsError = false;
    }

    public bool IsWebp => this._path.EndsWith(".webp", StringComparison.OrdinalIgnoreCase);
    
    public BitmapSource Image
    {
        get
        {
            if (_image == null && !_isLoading)
            {
                //LoadAsync();
            }
            return _image;
        }
    }

    public bool IsError { get; private set; }

    public string ErrorMessage { get; private set; }

    public Size ImageSize => new Size(Image.PixelWidth, Image.PixelHeight);

    public Size ViewPort => _viewPort;

    public ImageFormat ImageFormat =>
        _imageFileInfo.Extension.ToLower() switch
        {
            ".jpeg" => ImageFormat.JPEG,
            ".jpg" => ImageFormat.JPG,
            ".gif" => ImageFormat.GIF,
            ".bmp" => ImageFormat.BMP,
            ".tiff" => ImageFormat.TIFF,
            ".png" => ImageFormat.PNG,
            _ => ImageFormat.JPG
        };

    public string Name => _imageFileInfo.Name;

    public string FullName => _imageFileInfo.FullName;

    public bool IsLoading => _isLoading;

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

    public void DoLoadAsync()
    {
        if (!_isLoaded) 
            LoadAsync();
    }

    private async void LoadAsync()
    {
        _isLoading = true;
        OnImageChanged();

        PreviewLoadingThreadQueue.Add(Load);

        //await Task.Run(() => Load());
        //OnImageChanged();
    }

    private async Task Load()
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
                    if (_frameSize == null && !IsWebp)
                    {
                        var decoder =
                            BitmapDecoder.Create(
                                new Uri(_path),
                                BitmapCreateOptions.None,
                                BitmapCacheOption.None);
                        var frame = decoder.Frames[0];

                        _frameSize = new Size(frame.PixelWidth, frame.PixelHeight);
                    }
                    else if (_frameSize == null)
                    {
                        var decoder = new Imazen.WebP.SimpleDecoder();
                        var bytes = await File.ReadAllBytesAsync(_path);
                        var bitmap = decoder.DecodeFromBytes(bytes, bytes.Length);

                        _frameSize = new Size(bitmap.Width, bitmap.Height);
                    }


                    resizedSize
                        = ResizeImage(
                            _frameSize.Value,
                            new Size(_viewPort.Width, _viewPort.Height),
                            DefaultResizeType);
                }
            }

            var bi = await LoadBitmapImage(resizedSize);

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

    private async Task<BitmapImage> LoadBitmapImage(
        Size resizedSize,
        CancellationToken cancellationToken = default)
    {
        return await Task.Run(
            () =>
            {
                using var stream = new MemoryStream();
                var useStream = false;
                if (IsWebp)
                {
                    var decoder = new Imazen.WebP.SimpleDecoder();
                    var bytes = File.ReadAllBytes(_path);
                    var bitmap = decoder.DecodeFromBytes(bytes, bytes.Length);
                    bitmap.Save(stream, System.Drawing.Imaging.ImageFormat.Png);
                    stream.Seek(0, SeekOrigin.Begin);
                    useStream = true;
                }

                var bi = new BitmapImage();
                bi.BeginInit();

                if (!resizedSize.IsEmpty
                    && resizedSize.Width != 0
                    && resizedSize.Height != 0)
                {
                    bi.DecodePixelWidth = Convert.ToInt32(resizedSize.Width);
                    bi.DecodePixelHeight = Convert.ToInt32(resizedSize.Height);
                }

                bi.CacheOption = BitmapCacheOption.OnLoad;

                if (useStream)
                    bi.StreamSource = stream;
                else
                    bi.UriSource = new Uri(_path);

                bi.EndInit();
                bi.Freeze();

                return bi;
            },
            cancellationToken);
    }

    public event EventHandler ImageChanged;

    private void OnImageChanged()
    {
        ImageChanged?.Invoke(this, EventArgs.Empty);
    }

    private static Size ResizeImage(Size original, Size viewPort, ResizeType type)
    {
        return type switch
        {
            ResizeType.DownscaleToViewPort => DownscaleToViewPort(original, viewPort),
            ResizeType.DownscaleToViewPortWidth => DownscaleToViewPortWidth(original, viewPort),
            ResizeType.FitToViewPort => FitToViewPort(original, viewPort),
            ResizeType.FitToViewPortWidth => FitToViewPortWidth(original, viewPort),
            _ => original
        };
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
}
