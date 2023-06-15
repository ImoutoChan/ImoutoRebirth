using ImoutoRebirth.Lilin.Domain.FileInfoAggregate;

namespace ImoutoRebirth.Lilin.Application.Persistence;

public interface IFileNoteRepository
{
    Task<IReadOnlyCollection<FileNote>> GetForFile(Guid fileId, CancellationToken ct = default);

    Task Create(FileNote note);
    
    Task Update(FileNote note);

    Task Delete(FileNote note);
}
