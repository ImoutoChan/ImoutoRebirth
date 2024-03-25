using System.Globalization;

namespace ImoutoViewer.Extensions;

public static class ImageExtensions
{
    public static bool IsImage(this string filePath)
    {
        var ci = new CultureInfo("en-US");
        const string formats = @".jpg|.png|.jpeg|.bmp|.gif|.tiff|.webp|.jfif";

        return formats.Split('|').Any(item => filePath.EndsWith(item, true, ci));
    }
}
