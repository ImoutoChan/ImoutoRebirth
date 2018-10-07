using System.Collections.Generic;
using System.Threading.Tasks;
using ImoutoRebirth.Room.Core.Models;
using ImoutoRebirth.Room.DataAccess;
using ImoutoRebirth.Room.DataAccess.Models;

namespace ImoutoRebirth.Room.Core.Services
{
    public class OverseeService
    {
        private readonly IFileSystemActualizationService _fileSystemActualizationService;
        private readonly ICollectionRepository _collectionRepository;

        public OverseeService(
            IFileSystemActualizationService fileSystemActualizationService,
            ICollectionRepository collectionRepository)
        {
            _fileSystemActualizationService = fileSystemActualizationService;
            _collectionRepository = collectionRepository;
        }

        public async Task Oversee()
        {
            var collections = await LoadCollections();

            foreach (var overseedColleciton in collections)
                await _fileSystemActualizationService.PryColleciton(overseedColleciton);
        }

        private async Task<IReadOnlyCollection<OverseedColleciton>> LoadCollections()
        {
           return await _collectionRepository.GetAllOverseedCollecitons();
        }
    }
}