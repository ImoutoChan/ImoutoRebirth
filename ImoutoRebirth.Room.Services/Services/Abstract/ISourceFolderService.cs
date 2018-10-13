using System.Collections.Generic;
using System.Threading.Tasks;
using ImoutoRebirth.Room.Core.Models;
using ImoutoRebirth.Room.DataAccess.Models;

namespace ImoutoRebirth.Room.Core.Services.Abstract
{
    public interface ISourceFolderService
    {
        Task<IReadOnlyCollection<MoveInformation>> GetNewFiles(
            SourceFolder forSourceFolder);
    }
}