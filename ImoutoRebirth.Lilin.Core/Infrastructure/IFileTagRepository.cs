using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ImoutoRebirth.Lilin.Core.Models;
using ImoutoRebirth.Lilin.Core.Models.FileInfoAggregate;

namespace ImoutoRebirth.Lilin.Core.Infrastructure
{
    public interface IFileTagRepository
    {
        Task<Guid[]> SearchFiles(
            IReadOnlyCollection<TagSearchEntry> tagSearchEntries,
            uint? limit = 100,
            uint offset = 0);

        Task<uint> SearchFilesCount(IReadOnlyCollection<TagSearchEntry> tagSearchEntries);

        Task<IReadOnlyCollection<FileTag>> GetForFile(Guid fileId);
        
        Task Update(FileTag fileTag);

        Task Add(FileTag fileTag);

        Task Delete(FileTag fileTag);
    }
}