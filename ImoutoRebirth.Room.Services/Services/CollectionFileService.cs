using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ImoutoRebirth.Room.Core.Models;
using ImoutoRebirth.Room.Core.Services.Abstract;
using ImoutoRebirth.Room.DataAccess.Models;
using ImoutoRebirth.Room.DataAccess.Repositories.Abstract;
using ImoutoRebirth.Room.DataAccess.Repositories.Queries;

namespace ImoutoRebirth.Room.Core.Services
{
    public class CollectionFileService : ICollectionFileService
    {
        private const string DeletedDirectoryName = "deleted";
        private readonly ICollectionFileRepository _collectionFileRepository;
        private readonly IDestinationFolderRepository _destinationFolderRepository;
        private readonly IFileService _fileService;

        public CollectionFileService(
            ICollectionFileRepository collectionFileRepository,
            IFileService fileService,
            IDestinationFolderRepository destinationFolderRepository)
        {
            _collectionFileRepository = collectionFileRepository;
            _fileService = fileService;
            _destinationFolderRepository = destinationFolderRepository;
        }

        public async Task<Guid> SaveNew(
            MovedInformation movedInformation,
            Guid collectionId)
        {
            var newFile = new CollectionFile(Guid.NewGuid(),
                collectionId,
                movedInformation.MovedFileInfo.FullName,
                movedInformation.SystemFile.Md5,
                movedInformation.SystemFile.Size,
                movedInformation.SystemFile.File.FullName);

            await _collectionFileRepository.Add(newFile);

            return newFile.Id;
        }

        public async Task Delete(Guid id)
        {
            var found = await _collectionFileRepository.SearchByQuery(
                new CollectionFilesQuery(default, new[] {id}, default, Array.Empty<string>(), default, default));

            if (found.Any())
            {
                foreach (var file in found)
                {
                    await DeleteCollectionFile(file);
                }

                await _collectionFileRepository.Remove(id);
            }
        }

        private async Task DeleteCollectionFile(CollectionFile file)
        {
            var fileToDelete = new FileInfo(file.Path);


            var deletedDirectory = await GetDeletedFolder(fileToDelete, file.Id);

            var fileToDeleteDestination = new FileInfo(Path.Combine(deletedDirectory.FullName, fileToDelete.Name));

            _fileService.MoveFile(
                new SystemFile(fileToDelete, file.Md5, file.Size),
                ref fileToDeleteDestination);
        }

        private async Task<DirectoryInfo> GetDeletedFolder(FileInfo fileToDelete, Guid fileId)
        {
            var destinationFolderForFile = await _destinationFolderRepository.Get(fileId);

            if (destinationFolderForFile == null)
                return fileToDelete.Directory.CreateSubdirectory(DeletedDirectoryName);

            var destPath = destinationFolderForFile.GetDestinationDirectory();

            var isFileInDestinationDirectory = fileToDelete.Directory.FullName.ToLowerInvariant()
                .Contains(destPath.FullName.ToLowerInvariant());

            if (isFileInDestinationDirectory)
            {
                return destPath.CreateSubdirectory(DeletedDirectoryName);
            }

            return fileToDelete.Directory.CreateSubdirectory(DeletedDirectoryName);
        }
    }
}
