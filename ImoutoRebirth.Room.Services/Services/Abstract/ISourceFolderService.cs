using System.Collections.Generic;
using ImoutoRebirth.Room.Core.Models;
using ImoutoRebirth.Room.DataAccess.Models;

namespace ImoutoRebirth.Room.Core.Services.Abstract
{
    public interface ISourceFolderService
    {
        IReadOnlyCollection<MoveInformation> GetNewFiles(
            SourceFolder forSourceFolder, 
            HashSet<string> except);
    }
}