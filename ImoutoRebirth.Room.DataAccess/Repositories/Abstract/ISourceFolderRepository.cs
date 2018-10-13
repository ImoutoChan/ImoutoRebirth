using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ImoutoRebirth.Room.DataAccess.Models;

namespace ImoutoRebirth.Room.DataAccess.Repositories.Abstract
{
    public interface ISourceFolderRepository
    {
        Task<IReadOnlyCollection<SourceFolder>> Get(Guid collectionGuid);

        Task<SourceFolder> Add(SourceFolderCreateData createData);

        Task Remove(Guid guid);
    }
}