using System.Collections.Generic;
using System.Threading.Tasks;
using ImoutoRebirth.Room.Core.Services.Abstract;
using ImoutoRebirth.Room.DataAccess.Models;
using ImoutoRebirth.Room.DataAccess.Repositories.Abstract;

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

            foreach (var oversawCollection in collections)
                await _fileSystemActualizationService.PryCollection(oversawCollection);
        }

        private async Task<IReadOnlyCollection<OversawCollection>> LoadCollections()
        {
           return await _collectionRepository.GetAllOversawCollections();
        }
    }
}