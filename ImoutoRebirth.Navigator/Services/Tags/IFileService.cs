using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ImoutoRebirth.Navigator.Services.Tags
{
    interface IFileService
    {
        Task<IReadOnlyCollection<File>> SearchFiles(
            Guid? collectionId, 
            IReadOnlyCollection<SearchTag> tags, 
            int take, 
            int skip);

        Task<int> CountFiles(
            Guid? collectionId, 
            IReadOnlyCollection<SearchTag> tags);

        Task RemoveFile(Guid fileId);
    }
}