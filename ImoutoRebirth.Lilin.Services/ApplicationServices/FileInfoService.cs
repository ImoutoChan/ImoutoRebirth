using System;
using System.Linq;
using System.Threading.Tasks;
using ImoutoRebirth.Lilin.Core.Infrastructure;
using ImoutoRebirth.Lilin.Core.Models.FileInfoAggregate;

namespace ImoutoRebirth.Lilin.Services.ApplicationServices
{
    public class FileInfoService : IFileInfoService
    {
        private readonly IFileTagRepository _fileTagRepository;
        private readonly IFileNoteRepository _fileNoteRepository;

        public FileInfoService(IFileTagRepository fileTagRepository, IFileNoteRepository fileNoteRepository)
        {
            _fileTagRepository = fileTagRepository;
            _fileNoteRepository = fileNoteRepository;
        }

        public async Task<FileInfo> LoadFileAggregate(Guid fileId)
        {
            var tags = await _fileTagRepository.GetForFile(fileId);
            var notes = await _fileNoteRepository.GetForFile(fileId);

            return new FileInfo(tags, notes, fileId);
        }

        public async Task PersistFileAggregate(FileInfo file)
        {
            await PersistNotes(file);
            await PersistTags(file);
        }

        private async Task PersistNotes(FileInfo file)
        {
            var existingNotes = (await _fileNoteRepository.GetForFile(file.Id)).ToList();
            foreach (var newNote in file.Notes)
            {
                var existedNote = existingNotes.FirstOrDefault(x => x.IsSameIdentity(newNote));

                if (existedNote != null)
                {
                    await _fileNoteRepository.Update(existedNote.Note.Id, newNote.Note);
                    existingNotes.Remove(existedNote);
                }
                else
                {
                    await _fileNoteRepository.Add(newNote);
                }
            }

            foreach (var existingNote in existingNotes)
            {
                await _fileNoteRepository.Delete(existingNote.Note.Id);
            }
        }

        private async Task PersistTags(FileInfo file)
        {
            var existingTags = (await _fileTagRepository.GetForFile(file.Id)).ToList();
            foreach (var newTag in file.Tags)
            {
                var existedTag = existingTags.FirstOrDefault(x => x.IsSameIdentity(newTag));

                if (existedTag == null)
                {
                    await _fileTagRepository.Add(newTag);
                }
                else
                {
                    existingTags.Remove(existedTag);
                }
            }

            foreach (var existingTag in existingTags)
            {
                await _fileTagRepository.Delete(existingTag);
            }
        }
    }
}