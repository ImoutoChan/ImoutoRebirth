using System;
using System.Threading.Tasks;
using ImoutoRebirth.Room.Core.Models;
using ImoutoRebirth.Room.DataAccess;
using ImoutoRebirth.Room.DataAccess.Models;

namespace ImoutoRebirth.Room.Core.Services
{
    public class CollectionFileService : ICollectionFileService
    {
        private readonly ICollectionFileRepository _collectionFileRepository;

        public CollectionFileService(ICollectionFileRepository collectionFileRepository)
        {
            _collectionFileRepository = collectionFileRepository;
        }

        public async Task SaveNew(
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
        }
    }
}