using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ImoutoRebirth.Lilin.WebApi.Client;
using ImoutoRebirth.Room.WebApi.Client;

namespace ImoutoRebirth.Navigator.Services.Tags
{
    class FileService : IFileService
    {
        private readonly IImoutoRebirthLilinWebApiClient _lilinClient;
        private readonly IImoutoRebirthRoomWebApiClient _roomClient;

        public FileService(IImoutoRebirthLilinWebApiClient lilinClient, IImoutoRebirthRoomWebApiClient roomClient)
        {
            _lilinClient = lilinClient;
            _roomClient = roomClient;
        }

        public Task<IReadOnlyCollection<File>> SearchFiles(
            Guid? collectionId, 
            IReadOnlyCollection<SearchTag> tags, 
            int take, 
            int skip)
        {
            throw new NotImplementedException();
        }

        public Task<int> CountFiles(Guid? collectionId, IReadOnlyCollection<SearchTag> tags)
        {
            throw new NotImplementedException();
        }

        public Task RemoveFile(Guid fileId)
        {
            throw new NotImplementedException();
        }
    }
}