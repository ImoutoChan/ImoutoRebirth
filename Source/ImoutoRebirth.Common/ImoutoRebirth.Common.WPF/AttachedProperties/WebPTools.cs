using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using ImoutoRebirth.Common.WebP;

namespace ImoutoRebirth.Common.WPF.AttachedProperties;

public static class WebPTools
{
    public static readonly DependencyProperty ImagePathProperty = DependencyProperty.RegisterAttached(
        "ImagePath",
        typeof(string),
        typeof(WebPTools),
        new PropertyMetadata(null, OnWebPImagePathChanged));

    public static void SetImagePath(UIElement element, string value) => element.SetValue(ImagePathProperty, value);

    public static string GetImagePath(UIElement element) => (string)element.GetValue(ImagePathProperty);

    private static void OnWebPImagePathChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is Image image && e.NewValue is string path)
        {
            try
            {
                LoadAndSetWebPImage(image, path);
            }
            catch (Exception exception)
            {
                Debug.WriteLine(exception.Message);
            }
        }
    }

    private static void LoadAndSetWebPImage(Image imageControl, string path)
    {
        using var memoryStream = new MemoryStream();
        
        var decoder = new SimpleDecoder();
        var bytes = File.ReadAllBytes(path);
        var bitmap = decoder.DecodeFromBytes(bytes, bytes.Length);
        bitmap.Save(memoryStream, System.Drawing.Imaging.ImageFormat.Png);
        memoryStream.Seek(0, SeekOrigin.Begin);
        
        var bitmapImage = new BitmapImage();
        bitmapImage.BeginInit();
        bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
        bitmapImage.StreamSource = memoryStream;
        bitmapImage.EndInit();
        bitmapImage.Freeze();

        imageControl.Source = bitmapImage;
    }
}
