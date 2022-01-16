using System.Text.RegularExpressions;
using ImoutoRebirth.Room.Core.Models;
using ImoutoRebirth.Room.Core.Services.Abstract;
using ImoutoRebirth.Room.DataAccess.Models;
using ImoutoRebirth.Room.DataAccess.Repositories.Abstract;
using Microsoft.Extensions.Logging;

namespace ImoutoRebirth.Room.Core.Services
{
    public class SourceFolderService : ISourceFolderService
    {
        private readonly ILogger<SourceFolderService> _logger;
        private readonly IFileService _fileService;
        private readonly IImageService _imageService;
        private readonly ICollectionFileRepository _collectionFileRepository;
        private readonly ISourceTagsProvider _sourceTagsProvider;

        public SourceFolderService(
            ILogger<SourceFolderService> logger,
            IFileService fileService,
            IImageService imageService,
            ICollectionFileRepository collectionFileRepository,
            ISourceTagsProvider sourceTagsProvider)
        {
            _logger = logger;
            _fileService = fileService;
            _imageService = imageService;
            _collectionFileRepository = collectionFileRepository;
            _sourceTagsProvider = sourceTagsProvider;
        }

        public async Task<IReadOnlyCollection<MoveInformation>> GetNewFiles(SourceFolder forSourceFolder)
        {
            if (forSourceFolder == null)
                throw new ArgumentNullException(nameof(forSourceFolder));

            var extensions = forSourceFolder.SupportedExtensions;
            var directoryInfo = new DirectoryInfo(forSourceFolder.Path);

            if (!directoryInfo.Exists)
            {
                return Array.Empty<MoveInformation>();
            }

            var files = _fileService.GetFiles(directoryInfo, extensions)
                .OrderBy(x => x.LastWriteTimeUtc)
                .ToList();

            var uniqueFiles = files.DistinctBy(x => x.FullName).ToList();

            if (files.Count != uniqueFiles.Count)
            {
                _logger.LogWarning("{DuplicatesCount} duplicates received", files.Count - uniqueFiles.Count);
            }

            var newFiles = await FilterExistingFilePaths(forSourceFolder.CollectionId, uniqueFiles);
            var systemFiles = newFiles.Select(x => CreateSystemFile(x));

            var result = new List<MoveInformation>();

            foreach (var systemFile in systemFiles)
            {
                var prepared = await PrepareMove(forSourceFolder, systemFile);

                if (prepared == null)
                    continue;

                var duplicate = result.FirstOrDefault(x => x.SystemFile.Md5 == prepared.SystemFile.Md5);

                if (duplicate != null)
                {
                    _logger.LogWarning(
                        "Duplicate files in source folder: {File}, {DuplicateFile}",
                        duplicate.SystemFile.File.FullName,
                        prepared.SystemFile.File.FullName);

                    continue;
                }

                result.Add(prepared);
            }

            return result;
        }

        /// <summary>
        /// Filter out images that already in database if source used without destination
        /// </summary>
        /// <param name="collectionId">The collection which being processed</param>
        /// <param name="files">All found files</param>
        /// <returns>Files which aren't contained in the collection</returns>
        private async Task<List<FileInfo>> FilterExistingFilePaths(
            Guid collectionId,
            IEnumerable<FileInfo> files)
        {
            var newFiles = new List<FileInfo>();
            foreach (var fileInfo in files)
            {
                var exists = await _collectionFileRepository
                    .AnyWithPath(collectionId, fileInfo.FullName);

                if (exists)
                    continue;

                newFiles.Add(fileInfo);
            }

            return newFiles;
        }

        private async Task<MoveInformation?> PrepareMove(SourceFolder sourceFolder, SystemFile systemFile)
        {
            var fileInfo = systemFile.File;

            if (!_fileService.IsFileReady(fileInfo))
            {
                _logger.LogWarning("File is not ready or was removed");
                return null;
            }

            var moveInformation = new MoveInformation(systemFile);

            moveInformation.MoveProblem = await FindProblems(sourceFolder, systemFile);

            if (sourceFolder.ShouldAddTagFromFilename)
            {
                moveInformation.SourceTags.AddRange(_sourceTagsProvider.GetTagsFromName(fileInfo));
            }

            if (sourceFolder.ShouldCreateTagsFromSubfolders)
            {
                moveInformation.SourceTags.AddRange(_sourceTagsProvider.GetTagsFromPath(sourceFolder, fileInfo));
            }

            return moveInformation;
        }

        private SystemFile CreateSystemFile(FileInfo file)
        {
            var md5 = _fileService.GetMd5Checksum(file);

            return new SystemFile(file, md5, file.Length);
        }

        private async Task<MoveProblem> FindProblems(SourceFolder sourceDirectory, SystemFile systemFile)
        {
            var existingFile = await _collectionFileRepository
                .GetWithMd5(sourceDirectory.CollectionId, systemFile.Md5);

            if (existingFile != null)
            {
                _logger.LogWarning(
                    "File with the same md5 {Md5} already presented in the database. "
                        + "NewFile: {NewFile}. "
                        + "ExistingFile: {ExistingFile}.",
                    systemFile.Md5,
                    systemFile.File.FullName,
                    existingFile);

                return MoveProblem.AlreadyContains;
            }

            if (sourceDirectory.ShouldCheckFormat && !_imageService.IsImageCorrect(systemFile.File))
            {
                return MoveProblem.InvalidFormat;
            }

            if (sourceDirectory.ShouldCheckHashFromName)
            {
                if (!TryGetHashFromFileName(systemFile.File.Name, out var nameHash))
                {
                    return MoveProblem.WithoutHash;
                }

                if (!string.Equals(nameHash, systemFile.Md5, StringComparison.OrdinalIgnoreCase))
                {
                    return MoveProblem.IncorrectHash;
                }
            }

            return MoveProblem.None;
        }

        private static bool TryGetHashFromFileName(string name, out string? hash)
        {
            hash = null;
            var match = Regex.Match(name, "[0-9a-f]{32}", RegexOptions.IgnoreCase | RegexOptions.Compiled);

            if (!match.Success)
                return false;

            hash = match.Value;
            return true;
        }
    }
}
