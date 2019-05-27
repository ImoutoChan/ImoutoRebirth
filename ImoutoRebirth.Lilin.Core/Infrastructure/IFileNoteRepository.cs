using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ImoutoRebirth.Lilin.Core.Models;

namespace ImoutoRebirth.Lilin.Core.Infrastructure
{
    public interface IFileNoteRepository
    {
        Task Add(FileNote fileNote);

        Task<IReadOnlyCollection<FileNote>> GetForFile(Guid fileId);

        Task ClearForSource(Guid fileId, MetadataSource source);
    }
}