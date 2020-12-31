using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ImoutoRebirth.Room.Core.Services.Abstract;
using ImoutoRebirth.Room.DataAccess.Models;
using ImoutoRebirth.Room.DataAccess.Repositories.Abstract;
using ImoutoRebirth.Room.DataAccess.Repositories.Queries;
using Microsoft.Extensions.Logging;

namespace ImoutoRebirth.Room.Core.Services
{
    public class LocationTagsUpdaterService : ILocationTagsUpdaterService
    {
        private readonly ICollectionRepository _collectionRepository;
        private readonly ILogger _logger;
        private readonly ISourceTagsProvider _sourceTagsProvider;
        private readonly ICollectionFileRepository _collectionFileRepository;
        private readonly IFileService _fileService;
        private readonly IRemoteCommandService _remoteCommandService;

        public LocationTagsUpdaterService(
            ICollectionRepository collectionRepository,
            ILogger<OverseeService> logger,
            ISourceTagsProvider sourceTagsProvider,
            ICollectionFileRepository collectionFileRepository,
            IFileService fileService,
            IRemoteCommandService remoteCommandService)
        {
            _collectionRepository = collectionRepository;
            _logger = logger;
            _sourceTagsProvider = sourceTagsProvider;
            _collectionFileRepository = collectionFileRepository;
            _fileService = fileService;
            _remoteCommandService = remoteCommandService;
        }

        /// <summary>
        /// Updates location tags of all files in collections, where:
        ///     1. Destination Folder is null;
        ///     2. Source Folder is marked with ShouldAddTagFromFilename flag.
        /// </summary>
        public async Task UpdateLocationTags()
        {
            try
            {
                var collections = await LoadCollections();

                foreach (var collection in collections)
                {
                    if (collection.DestinationFolder.GetDestinationDirectory() != null)
                        continue;

                    foreach (var sourceFolder in collection.SourceFolders)
                    {
                        if (!sourceFolder.ShouldAddTagFromFilename && !sourceFolder.ShouldCreateTagsFromSubfolders)
                            continue;

                        await UpdateLocationTagsInSourceFolder(sourceFolder);
                    }
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, "An error occurred while updating local tags");
            }
        }

        private async Task UpdateLocationTagsInSourceFolder(SourceFolder sourceFolder)
        {
            var allFiles = _fileService.GetFiles(
                new DirectoryInfo(sourceFolder.Path),
                sourceFolder.SupportedExtensions);

            _logger.LogInformation(
                "Updating location tags for {SourceFolder}, found {Count} new files",
                sourceFolder.Path,
                allFiles.Count);
            
            foreach (var file in allFiles)
            {
                var foundFiles = await _collectionFileRepository.SearchByQuery(
                    new CollectionFilesQuery(
                        default,
                        Array.Empty<Guid>(),
                        file.FullName,
                        Array.Empty<string>(),
                        1,
                        0));
                
                var foundFile = foundFiles.FirstOrDefault();

                if (foundFile == default)
                {
                    _logger.LogWarning(
                        "File {FilePath} was not found in collection",
                        file.FullName);
                    continue;
                }

                IEnumerable<string> tags = Array.Empty<string>();
                
                if (sourceFolder.ShouldAddTagFromFilename)
                    tags = tags.Union(_sourceTagsProvider.GetTagsFromName(file));
                
                if (sourceFolder.ShouldCreateTagsFromSubfolders)
                    tags = tags.Union(_sourceTagsProvider.GetTagsFromPath(sourceFolder, file));

                var readyTags = tags.ToList();
                
                _logger.LogInformation(
                    "Saving location tags for {FileId} {FilePath} {Tags}",
                    foundFile.Id,
                    foundFile.Path,
                    readyTags);
                
                await _remoteCommandService.SaveTags(foundFile.Id, readyTags);
            }
        }

        private async Task<IReadOnlyCollection<OversawCollection>> LoadCollections() 
            => await _collectionRepository.GetAllOversaw();
    }
}