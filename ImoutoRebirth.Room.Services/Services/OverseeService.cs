using System.Linq;
using ImoutoRebirth.Room.Core.Models;
using ImoutoRebirth.Room.Core.Services.Abstract;

namespace ImoutoRebirth.Room.Core.Services
{
    /// <summary>
    /// Collection monitoring
    /// </summary>
    public class OverseeService
    {
        private readonly ISourceFolderService _sourceFolderService;
        private readonly IDestinationFolderService _destinationFolderService;

        public OverseeService(
            ISourceFolderService sourceFolderService,
            IDestinationFolderService destinationFolderService)
        {
            _sourceFolderService = sourceFolderService;
            _destinationFolderService = destinationFolderService;
        }

        public void PryColleciton(OverseedColleciton overseedColleciton)
        {
            foreach (var overseedCollecitonSourceFolder in overseedColleciton.SourceFolders)
            {
                var newFiles = _sourceFolderService.GetNewFiles(
                    overseedCollecitonSourceFolder, 
                    overseedColleciton.ExistedFiles);

                var moved = newFiles
                   .Select(x => _destinationFolderService.Move(overseedColleciton.DestinationFolder, x));

                //foreach (var movedInformation in moved.Where(x => x.RequireSave))
                //    _collectionFileService.SaveNew(movedInformation);
            }
        }
    }
}