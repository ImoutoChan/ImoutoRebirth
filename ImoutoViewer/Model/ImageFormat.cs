using System;
using System.Collections.Generic;
using System.Linq;

namespace ImoutoViewer.Model
{
    enum ImageFormat
    {
        JPEG,
        JPG,
        PNG,
        BMP,
        TIFF,
        GIF
    }

    static class ImageFormats
    {
        /// <summary>
        /// Get supported formats list in string format.
        /// </summary>
        /// <returns>Strings: "jpg", "png", etc</returns>
        public static IEnumerable<string> GetSupportedFormatsList()
        {
            return Enum.GetNames(typeof(ImageFormat)).Select(x => "." + x.ToLower()).ToList();
        }
    }
}