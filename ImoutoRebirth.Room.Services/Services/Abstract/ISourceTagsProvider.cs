using System.Collections.Generic;
using System.IO;
using ImoutoRebirth.Room.DataAccess.Models;

namespace ImoutoRebirth.Room.Core.Services.Abstract
{
    public interface ISourceTagsProvider
    {
        IReadOnlyCollection<string> GetTagsFromName(FileInfo fileInfo);
        
        IReadOnlyCollection<string> GetTagsFromPath(SourceFolder sourceDirectory, FileInfo fileInfo);
    }
}