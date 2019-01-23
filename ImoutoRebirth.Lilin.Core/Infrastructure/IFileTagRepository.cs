using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ImoutoRebirth.Lilin.Core.Models;

namespace ImoutoRebirth.Lilin.Core.Infrastructure
{
    public interface IFileTagRepository : IRepository
    {
        Task Add(FileTagBind fileTag);

        Task<Guid[]> SearchFiles(
            IReadOnlyCollection<TagSearchEntry> tagSearchEntries,
            uint? limit = 100,
            uint offset = 0);

        Task<uint> SearchFilesCount(IReadOnlyCollection<TagSearchEntry> tagSearchEntries);

        Task<IReadOnlyCollection<FileTag>> GetForFile(Guid fileId);

        Task ClearForSource(Guid fileId, MetadataSource source);
    }
}