using System;
using System.Threading.Tasks;
using ImoutoRebirth.Room.DataAccess.Models;

namespace ImoutoRebirth.Room.DataAccess.Repositories.Abstract
{
    public interface IDestinationFolderRepository
    {
        Task<DestinationFolder> Get(Guid collectionGuid);

        Task<DestinationFolder> AddOrReplace(DestinationFolderCreateData createData);

        Task Remove(Guid guid);
    }
}