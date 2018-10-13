using System.Linq;
using System.Threading.Tasks;
using ImoutoRebirth.Room.Core.Services.Abstract;
using ImoutoRebirth.Room.DataAccess;
using ImoutoRebirth.Room.DataAccess.Models;

namespace ImoutoRebirth.Room.Core.Services
{
    public class FileSystemActualizationService : IFileSystemActualizationService
    {
        private readonly ISourceFolderService      _sourceFolderService;
        private readonly IDestinationFolderService _destinationFolderService;
        private readonly ICollectionFileService    _collectionFileService;
        private readonly IDbStateService           _dbStateService;

        public FileSystemActualizationService(
            ISourceFolderService sourceFolderService,
            IDestinationFolderService destinationFolderService,
            ICollectionFileService collectionFileService,
            IDbStateService dbStateService)
        {
            _sourceFolderService = sourceFolderService;
            _destinationFolderService = destinationFolderService;
            _collectionFileService = collectionFileService;
            _dbStateService = dbStateService;
        }

        public async Task PryCollection(OversawCollection oversawCollection)
        {
            foreach (var collectionSourceFolder in oversawCollection.SourceFolders)
            {
                var newFiles = await _sourceFolderService.GetNewFiles(collectionSourceFolder);

                var moved = newFiles
                   .Select(x => _destinationFolderService.Move(oversawCollection.DestinationFolder, x));


                var tasks = moved.Where(x => x.RequireSave)
                                 .Select(x => _collectionFileService.SaveNew(x, oversawCollection.Collection.Id));

                await Task.WhenAll(tasks);

                await _dbStateService.SaveChanges();
            }
        }
    }
}