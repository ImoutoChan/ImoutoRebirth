using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using ImoutoRebirth.Common;
using ImoutoRebirth.Room.Core.Models;
using ImoutoRebirth.Room.Core.Services.Abstract;
using ImoutoRebirth.Room.DataAccess;
using ImoutoRebirth.Room.DataAccess.Models;
using Microsoft.Extensions.Logging;

namespace ImoutoRebirth.Room.Core.Services
{
    public class FileSystemActualizationService : IFileSystemActualizationService
    {
        private readonly ISourceFolderService _sourceFolderService;
        private readonly IDestinationFolderService _destinationFolderService;
        private readonly ICollectionFileService _collectionFileService;
        private readonly IDbStateService _dbStateService;
        private readonly IRemoteCommandService _remoteCommandService;
        private readonly ILogger<FileSystemActualizationService> _logger;

        public FileSystemActualizationService(
            ISourceFolderService sourceFolderService,
            IDestinationFolderService destinationFolderService,
            ICollectionFileService collectionFileService,
            IDbStateService dbStateService,
            IRemoteCommandService remoteCommandService,
            ILogger<FileSystemActualizationService> logger)
        {
            _sourceFolderService = sourceFolderService;
            _destinationFolderService = destinationFolderService;
            _collectionFileService = collectionFileService;
            _dbStateService = dbStateService;
            _remoteCommandService = remoteCommandService;
            _logger = logger;
        }

        public async Task PryCollection(OversawCollection oversawCollection)
        {
            _logger.LogTrace("Prying collection {CollectionName}", oversawCollection.Collection.Name);

            foreach (var collectionSourceFolder in oversawCollection.SourceFolders)
            {
                await ProcessSourceFolder(oversawCollection, collectionSourceFolder);
            }
        }

        private async Task ProcessSourceFolder(OversawCollection oversawCollection, SourceFolder collectionSourceFolder)
        {
            _logger.LogTrace("Looking at {SourceFolderPath}...", collectionSourceFolder.Path);

            var newFiles = await _sourceFolderService.GetNewFiles(collectionSourceFolder);

            if (!newFiles.Any())
            {
                return;
            }

            _logger.LogInformation("{NewFilesCount} new files found", newFiles.Count);

            var movedFiles = await MoveFiles(newFiles, oversawCollection).ToArrayAsync();
            
            await _dbStateService.SaveChanges();

            _logger.LogInformation("{NewFilesSavedCount} files saved", movedFiles.Length);

            await movedFiles.Select(x => _remoteCommandService.UpdateMetadataRequest(x.FileId, x.Md5))
                            .WhenAll();

            _logger.LogDebug("Update metadata requests are sent");
        }

        private async IAsyncEnumerable<(Guid FileId, string Md5)> MoveFiles(
            IEnumerable<MoveInformation> preparedFiles,
            OversawCollection oversawCollection)
        {
            foreach (var moveInformation in preparedFiles)
            {
                var movedInformation = MoveFile(oversawCollection, moveInformation);

                if (!movedInformation.RequireSave)
                    continue;

                var fileId = await _collectionFileService.SaveNew(movedInformation, oversawCollection.Collection.Id);
                var md5 = moveInformation.SystemFile.Md5;

                yield return (fileId, md5);
            }
        }

        private MovedInformation MoveFile(OversawCollection oversawCollection, MoveInformation moveInformation)
        {
            using (_logger.BeginScope(
                "Processing new file in source: {NewFile}, {MoveProblem}, {@SourceTags}",
                moveInformation.SystemFile.File.FullName,
                moveInformation.MoveProblem,
                moveInformation.SourceTags))
            {
                try
                {
                    return _destinationFolderService.Move(oversawCollection.DestinationFolder, moveInformation);
                }
                catch (Exception e)
                {
                    _logger.LogError(e, "Error occured while moving file");
                    return new MovedInformation(moveInformation, false, moveInformation.SystemFile.File);
                }
            }
        }
    }
}