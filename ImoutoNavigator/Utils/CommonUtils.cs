using System;
using System.IO;
using System.Security.Cryptography;

namespace ImoutoNavigator.Utils
{
    static class Util
    {
        public static string GetMd5Checksum(FileInfo targetFile)
        {
            if (!targetFile.Exists)
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
    }
}
