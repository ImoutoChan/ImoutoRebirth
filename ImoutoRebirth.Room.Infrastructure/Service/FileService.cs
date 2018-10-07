using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using ImoutoRebirth.Room.Core.Services.Abstract;

namespace ImoutoRebirth.Room.Infrastructure.Service
{
    public class FileService : IFileService
    {
        public IReadOnlyCollection<FileInfo> GetFiles(
            DirectoryInfo directoryInfo,
            IReadOnlyCollection<string> supportedExtensions,
            SearchOption searchOption = SearchOption.TopDirectoryOnly)
        {
            return supportedExtensions?.Any() != true
                ? directoryInfo.GetFiles("*.*", searchOption)
                : supportedExtensions.Select(x => "*." + x)
                                     .SelectMany(x => directoryInfo.EnumerateFiles(x, searchOption))
                                     .ToArray();
        }

        /// <summary>
        /// Check availability of file by trying to seizing exclusive access.
        /// </summary>
        /// <param name="fileInfo">File to check availability.</param>
        /// <returns>Indicates whenever file is available for exclusive access.</returns>
        public bool IsFileReady(FileInfo fileInfo)
        {
            // If the file can be opened for exclusive access it means that the file
            // is no longer locked by another process.
            try
            {
                using (fileInfo.Open(FileMode.Open, FileAccess.Read, FileShare.None))
                {
                    return true;
                }
            }
            catch (Exception)
            {
                return false;
            }
        }

        public string GetMd5Checksum(FileInfo fileInfo)
        {
            using (var md5 = MD5.Create())
            {
                using (var stream = fileInfo.OpenRead())
                {
                    return BitConverter.ToString(md5.ComputeHash(stream)).Replace("-", "").ToLower();
                }
            }
        }
    }
}