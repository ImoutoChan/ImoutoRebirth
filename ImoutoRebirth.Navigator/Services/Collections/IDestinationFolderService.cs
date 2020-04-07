using System;
using System.Threading.Tasks;

namespace ImoutoRebirth.Navigator.Services.Collections
{
    public interface IDestinationFolderService
    {
        Task<DestinationFolder> GetDestinationFolderAsync(Guid collectionId);

        Task<DestinationFolder> AddOrUpdateDestinationFolderAsync(DestinationFolder destinationFolder);

        Task DeleteDestinationFolderAsync(Guid collectionId, Guid destinationFolderId);
    }
}