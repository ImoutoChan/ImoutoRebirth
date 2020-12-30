using System.Collections.Generic;
using System.IO;
using ImoutoRebirth.Room.DataAccess.Models;

namespace ImoutoRebirth.Room.Core.Services.Abstract
{
    public interface ISourceTagsProvider
    {
        IReadOnlyCollection<string> GetTags(SourceFolder sourceDirectory, FileInfo fileInfo);
    }
}