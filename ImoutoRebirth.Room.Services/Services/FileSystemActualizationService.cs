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
        private readonly IRemoteCommandService             _remoteCommandService;

        public FileSystemActualizationService(
            ISourceFolderService sourceFolderService,
            IDestinationFolderService destinationFolderService,
            ICollectionFileService collectionFileService,
            IDbStateService dbStateService, 
            IRemoteCommandService remoteCommandService)
        {
            _sourceFolderService = sourceFolderService;
            _destinationFolderService = destinationFolderService;
            _collectionFileService = collectionFileService;
            _dbStateService = dbStateService;
            _remoteCommandService = remoteCommandService;
        }

        public async Task PryCollection(OversawCollection oversawCollection)
        {
            foreach (var collectionSourceFolder in oversawCollection.SourceFolders)
            {
                var newFiles = await _sourceFolderService.GetNewFiles(collectionSourceFolder);

                var moved = newFiles
                   .Select(x => _destinationFolderService.Move(oversawCollection.DestinationFolder, x));
                

                var tasks = moved.Where(x => x.RequireSave)
                                 .Select(x => (GuidTask: _collectionFileService.SaveNew(x, oversawCollection.Collection.Id), x.SystemFile.Md5))
                                 .ToList();

                await Task.WhenAll(tasks.Select(x => x.GuidTask));

                await _dbStateService.SaveChanges();

                await Task.WhenAll(
                    tasks.Select(x => _remoteCommandService.UpdateMetadataRequest(x.GuidTask.Result, x.Md5))
                );
            }
        }
    }
}