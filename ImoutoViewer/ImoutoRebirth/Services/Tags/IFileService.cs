using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ImoutoViewer.ImoutoRebirth.Services.Tags.Model;

namespace ImoutoViewer.ImoutoRebirth.Services.Tags
{
    internal interface IFileService
    {
        Task<IReadOnlyCollection<File>> SearchFiles(string md5, CancellationToken token = default);

        Task<IReadOnlyCollection<File>> SearchFiles(
            Guid? collectionId, 
            IReadOnlyCollection<SearchTag> tags,
            CancellationToken cancellationToken);

        Task<int> CountFiles(
            Guid? collectionId, 
            IReadOnlyCollection<SearchTag> tags,
            CancellationToken cancellationToken);

        Task RemoveFile(Guid fileId);
    }
}