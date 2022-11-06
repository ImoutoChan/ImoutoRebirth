using ImoutoRebirth.Lilin.Core.Models;
using ImoutoRebirth.Lilin.Core.Models.FileInfoAggregate;

namespace ImoutoRebirth.Lilin.Core.Infrastructure;

public interface IFileNoteRepository
{
    Task<IReadOnlyCollection<FileNote>> GetForFile(Guid fileId, CancellationToken ct);

    Task Add(FileNote fileNote);
    
    Task Update(Guid noteId, Note note);

    Task Delete(Guid noteId);
}
