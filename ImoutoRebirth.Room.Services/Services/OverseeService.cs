using System.Linq;
using System.Threading.Tasks;
using ImoutoRebirth.Room.Core.Models;
using ImoutoRebirth.Room.Core.Services.Abstract;
using ImoutoRebirth.Room.DataAccess;

namespace ImoutoRebirth.Room.Core.Services
{
    public class OverseeService
    {
        private readonly ISourceFolderService _sourceFolderService;
        private readonly IDestinationFolderService _destinationFolderService;
        private readonly ICollectionFileService _collectionFileService;
        private readonly IDbStateService _dbStateService;

        public OverseeService(
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

        public async Task PryColleciton(OverseedColleciton overseedColleciton)
        {
            foreach (var overseedCollecitonSourceFolder in overseedColleciton.SourceFolders)
            {
                var newFiles = _sourceFolderService.GetNewFiles(
                    overseedCollecitonSourceFolder, 
                    overseedColleciton.ExistedFiles);

                var moved = newFiles
                   .Select(x => _destinationFolderService.Move(overseedColleciton.DestinationFolder, x));


                var tasks = moved.Where(x => x.RequireSave)
                                 .Select(x => _collectionFileService.SaveNew(x, overseedColleciton.Collection.Id));

                await Task.WhenAll(tasks);

                await _dbStateService.SaveChanges();
            }
        }
    }
}