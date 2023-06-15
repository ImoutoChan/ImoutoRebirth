using ImoutoRebirth.Lilin.Application.Persistence;
using ImoutoRebirth.Lilin.Domain.FileInfoAggregate;

namespace ImoutoRebirth.Lilin.Infrastructure.Repositories;

internal class FileInfoRepository : IFileInfoRepository
{
    private readonly IFileTagRepository _fileTagRepository;
    private readonly IFileNoteRepository _fileNoteRepository;

    public FileInfoRepository(IFileTagRepository fileTagRepository, IFileNoteRepository fileNoteRepository)
    {
        _fileTagRepository = fileTagRepository;
        _fileNoteRepository = fileNoteRepository;
    }

    public async Task<FileInfo> Get(Guid fileId, CancellationToken ct)
    {
        var tags = await _fileTagRepository.GetForFile(fileId, ct);
        var notes = await _fileNoteRepository.GetForFile(fileId, ct);

        return new FileInfo(tags, notes, fileId);
    }

    public async Task Save(FileInfo file)
    {
        await SaveNotes(file);
        await SaveTags(file);
    }

    private async Task SaveNotes(FileInfo file)
    {
        var existingNotes = (await _fileNoteRepository.GetForFile(file.FileId)).ToList();
        
        foreach (var newNote in file.Notes)
        {
            var existedNote = existingNotes.FirstOrDefault(x => x.IsSameIdentity(newNote));
            
            if (existedNote != null)
                existingNotes.Remove(existedNote);
            
            var wasChanged = !existedNote?.IsSameContent(newNote);
            var shouldUpdate = existedNote != null && wasChanged == true;
            var shouldCreate = !shouldUpdate && existedNote == null;

            if (shouldUpdate)
            {
                await _fileNoteRepository.Update(newNote);
            }
            else if (shouldCreate)
            {
                await _fileNoteRepository.Create(newNote);
            }
        }

        var obsoleteNotes = existingNotes;
        foreach (var obsoleteNote in obsoleteNotes)
        {
            await _fileNoteRepository.Delete(obsoleteNote);
        }
    }

    private async Task SaveTags(FileInfo file)
    {
        var existingTags = (await _fileTagRepository.GetForFile(file.FileId)).ToList();
        
        foreach (var newTag in file.Tags)
        {
            var existedTag = existingTags.FirstOrDefault(x => x.Equals(newTag));

            if (existedTag != null)
            {
                existingTags.Remove(existedTag);
            }
            else
            {
                await _fileTagRepository.Add(newTag);
            }
        }

        foreach (var existingTag in existingTags)
        {
            await _fileTagRepository.Delete(existingTag);
        }
    }
}
