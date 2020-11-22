using System.Globalization;
using System.Linq;

namespace ImoutoViewer.Extensions
{
    public static class ImageExtensions
    {
        public static bool IsImage(this string filePath)
        {
            var ci = new CultureInfo("en-US");
            const string formats = @".jpg|.png|.jpeg|.bmp|.gif|.tiff";

            return formats.Split('|').Any(item => filePath.EndsWith(item, true, ci));
        }
    }
}