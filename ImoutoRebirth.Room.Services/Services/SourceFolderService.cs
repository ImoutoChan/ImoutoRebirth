using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
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

        public SourceFolderService(
            ILogger<SourceFolderService> logger,
            IFileService fileService,
            IImageService imageService,
            ICollectionFileRepository collectionFileRepository)
        {
            _logger = logger;
            _fileService = fileService;
            _imageService = imageService;
            _collectionFileRepository = collectionFileRepository;
        }

        public async Task<IReadOnlyCollection<MoveInformation>> GetNewFiles(SourceFolder forSourceFolder)
        {
            if (forSourceFolder == null)
                throw new ArgumentNullException(nameof(forSourceFolder));

            var extensions = forSourceFolder.SupportedExtensions;
            var directoryInfo = new DirectoryInfo(forSourceFolder.Path);

            var files = _fileService.GetFiles(directoryInfo, extensions);

            var newFiles = await FilterExistingFiles(forSourceFolder, files);

            return newFiles
                        .Select(CreateSystemFile)
                        .Select(x => PrepareMove(forSourceFolder, x))
                        .Where(x => x != null)
                        .ToArray();
        }

        private async Task<List<FileInfo>> FilterExistingFiles(
            SourceFolder forSourceFolder,
            IReadOnlyCollection<FileInfo> files)
        {
            var newFiles = new List<FileInfo>();
            foreach (var fileInfo in files)
            {
                var exists = await _collectionFileRepository.HasFile(forSourceFolder.CollectionId, fileInfo.FullName);

                if (exists)
                    continue;

                newFiles.Add(fileInfo);
            }

            return newFiles;
        }

        private MoveInformation PrepareMove(SourceFolder sourceFolder, SystemFile systemFile)
        {
            var fileInfo = systemFile.File;


            if (!_fileService.IsFileReady(fileInfo))
            {
                _logger.LogWarning("File is not ready or was removed");
                return null;
            }
            
            var moveInformation = new MoveInformation(systemFile);

            moveInformation.MoveProblem = FindProblems(sourceFolder, systemFile);

            if (sourceFolder.ShouldAddTagFromFilename)
            {
                moveInformation.SourceTags.AddRange(GetTags(sourceFolder, fileInfo));
            }

            if (sourceFolder.ShouldCreateTagsFromSubfolders)
            {
                moveInformation.SourceTags.Add(fileInfo.Name);
            }

            return moveInformation;
        }

        private SystemFile CreateSystemFile(FileInfo file)
        {
            var md5 = _fileService.GetMd5Checksum(file);

            return new SystemFile(file, md5, file.Length);
        }
        
        private IReadOnlyCollection<string> GetTags(SourceFolder sourceDirectory, FileInfo fileInfo)
        {
            string[] GetPathParts(string path)
                => path.Split(new[]
                              {
                                  Path.VolumeSeparatorChar,
                                  Path.AltDirectorySeparatorChar,
                                  Path.DirectorySeparatorChar,
                                  Path.PathSeparator
                              },
                              StringSplitOptions.RemoveEmptyEntries);

            var sourcePathEnries = GetPathParts(sourceDirectory.Path);
            var filePathEntries = GetPathParts(fileInfo.Directory.FullName);

            return filePathEntries.Except(sourcePathEnries).ToArray();
        }

        private MoveProblem FindProblems(SourceFolder sourceDirectory, SystemFile systemFile)
        {
            if (sourceDirectory.ShouldCheckFormat
                && !_imageService.IsImageCorrect(systemFile.File))
            {
                return MoveProblem.InvalidFormat;
            }

            if (sourceDirectory.ShouldCheckHashFromName)
            {
                if (!TryGetHashFromFileName(systemFile.File.Name, out var nameHash))
                {
                    return MoveProblem.WithoutHash;
                }
                else if (!string.Equals(nameHash, systemFile.Md5, StringComparison.OrdinalIgnoreCase))
                {
                    return MoveProblem.IncorrectHash;
                }
            }

            return MoveProblem.None;
        }

        private static bool TryGetHashFromFileName(string name, out string hash)
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
