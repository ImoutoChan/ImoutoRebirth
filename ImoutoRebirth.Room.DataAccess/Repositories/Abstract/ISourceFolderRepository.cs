using ImoutoRebirth.Room.DataAccess.Models;

namespace ImoutoRebirth.Room.DataAccess.Repositories.Abstract
{
    public interface ISourceFolderRepository
    {
        Task<IReadOnlyCollection<SourceFolder>> Get(Guid collectionGuid);

        Task<SourceFolder> Add(SourceFolderCreateData createData);
        
        Task<SourceFolder> Update(SourceFolderUpdateData updateData);

        Task Remove(Guid guid);
    }
}