using System.Collections.Generic;
using System.IO;

namespace ImoutoRebirth.Room.Core.Services.Abstract
{
    public interface IFileService
    {
        string GetMd5Checksum(FileInfo fileInfo);
        bool IsFileReady(FileInfo fileInfo);

        IReadOnlyCollection<FileInfo> GetFiles(
            DirectoryInfo directoryInfo,
            IReadOnlyCollection<string> supportedExtensions,
            SearchOption searchOption = SearchOption.AllDirectories);
    }
}
