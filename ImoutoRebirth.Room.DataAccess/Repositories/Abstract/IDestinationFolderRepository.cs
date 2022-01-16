using ImoutoRebirth.Room.DataAccess.Models;

namespace ImoutoRebirth.Room.DataAccess.Repositories.Abstract
{
    public interface IDestinationFolderRepository
    {
        Task<CustomDestinationFolder?> Get(Guid collectionGuid);

        Task<CustomDestinationFolder> AddOrReplace(DestinationFolderCreateData createData);

        Task Remove(Guid guid);
    }
}
