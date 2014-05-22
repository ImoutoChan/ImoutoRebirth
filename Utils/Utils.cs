using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;

namespace Utils
{
    public static class Util
    {
        public static string GetMd5Checksum(FileInfo targetFile)
        {
            if (!targetFile.SpeedExists())
            {
                throw new ArgumentException("File does not exist.");
            }

            using (var md5 = MD5.Create())
            {
                using (var stream = targetFile.OpenRead())
                {
                    return BitConverter.ToString(md5.ComputeHash(stream)).Replace("-", "").ToLower();
                }
            }
        }

        #region Speedup FileInfo.Exists

        [DllImport("Shlwapi.dll", SetLastError = true, CharSet = CharSet.Auto)]
        private extern static bool PathFileExists(StringBuilder path);

        /// <summary>
        /// Speed raised up only on non existing files.
        /// </summary>
        public static bool SpeedExists(this FileInfo fi)
        {
            StringBuilder builder = new StringBuilder();
            builder.Append(fi.FullName);
            return PathFileExists(builder);
        }

        #endregion

        #region Check file is image

        public static bool IsImage(this string filePath)
        {
            var ci = new CultureInfo("en-US");
            const string formats = @".jpg|.png|.jpeg|.bmp|.gif|.tiff";

            return formats.Split('|').Any(item => filePath.EndsWith(item, true, ci));
        }

        #endregion Check file is image

        public static IEnumerable<DirectoryInfo> GetDirectories(this DirectoryInfo source, bool isRecursive = false)
        {
            var result = new List<DirectoryInfo>();

            try
            {
                #region Check on softlink !!Very slow
                if ((File.GetAttributes(source.FullName) & FileAttributes.ReparsePoint) == FileAttributes.ReparsePoint)
                {
                    return result;
                }
                #endregion

                foreach (DirectoryInfo folder in source.GetDirectories().OrderBy(x=>x.Name))
                {
                    result.Add(folder);

                    if (isRecursive)
                    {
                        result.AddRange(GetDirectories(folder, true));
                    }
                }
            }
            catch (UnauthorizedAccessException) { }

            return result;
        }

        public static void ForEach<T>(this IEnumerable<T> enumerable, Action<T> action)
        {
            foreach (var cur in enumerable)
            {
                action(cur);
            }
        }
    }
}
