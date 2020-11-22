using System;
using System.Collections.Generic;
using System.Linq;

namespace ImoutoViewer.ViewModel.SettingsModels
{
    public static class SupportedFormatsExtensions
    {
        /// <summary>
        /// Get supported formats list in string format.
        /// </summary>
        /// <returns>Strings: "jpg", "png", etc</returns>
        public static IEnumerable<string> GetSupportedFormatsList(Type extensionEnum)
        {
            return Enum.GetNames(extensionEnum).Select(x => "." + x.ToLower()).ToList();
        }

        /// <summary>
        /// Check if string ends with image extension.
        /// </summary>
        /// <param name="path">Path to file.</param>
        /// <returns></returns>
        public static bool IsImage(this string path)
        {
            return GetSupportedFormatsList(typeof(ImageFormat)).Any(x => path.ToLower().EndsWith(x));
        }

        /// <summary>
        /// Check if string ends with video extension.
        /// </summary>
        /// <param name="path">Path to file.</param>
        /// <returns></returns>
        public static bool IsVideo(this string path)
        {
            return GetSupportedFormatsList(typeof(VideoFormat)).Any(x => path.ToLower().EndsWith(x));
        }

        /// <summary>
        /// Check if string ends with previewable video extension.
        /// </summary>
        /// <param name="path">Path to file.</param>
        /// <returns></returns>
        public static bool IsPreviewableVideo(this string path)
        {
            return GetSupportedFormatsList(typeof(PreviewableVideoFormat)).Any(x => path.ToLower().EndsWith(x));
        }
    }
}