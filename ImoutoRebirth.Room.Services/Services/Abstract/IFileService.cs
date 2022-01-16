using ImoutoRebirth.Room.Core.Models;

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

        /// <summary>
        /// Move file from current to desired location.
        /// </summary>
        /// <param name="oldFile">Current file location.</param>
        /// <param name="newFile">Desired file location.</param>
        /// <returns>Should file be registered.</returns>
        bool MoveFile(SystemFile oldFile, ref FileInfo newFile);
    }
}
