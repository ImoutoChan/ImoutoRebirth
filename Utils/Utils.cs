using System;
using System.IO;
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

        public static bool SpeedExists(this FileInfo fi)
        {
            StringBuilder builder = new StringBuilder();
            builder.Append(fi.FullName);
            return PathFileExists(builder);
        }

        #endregion
    }
}
