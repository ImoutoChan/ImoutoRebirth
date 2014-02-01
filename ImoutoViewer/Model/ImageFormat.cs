using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Documents;

namespace ImoutoViewer.Model
{
    public enum ImageFormat
    {
        JPEG,
        JPG,
        PNG,
        BMP,
        TIFF,
        GIF
    }

    public static class ImageFormats
    {
        /// <summary>
        /// Get supported formats list in string format.
        /// </summary>
        /// <returns>Strings: "jpg", "png", etc</returns>
        public static IEnumerable<string> GetSupportedFormatsList()
        {
            return Enum.GetNames(typeof (ImageFormat)).Select(x => x.ToLower()).ToList();
        }
    }
}