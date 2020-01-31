using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ImoutoRebirth.Lilin.Core.Infrastructure;
using ImoutoRebirth.Lilin.Core.Models;
using ImoutoRebirth.Lilin.Core.Models.FileInfoAggregate;
using ImoutoRebirth.Lilin.DataAccess;
using ImoutoRebirth.Lilin.Infrastructure.Mappers;
using Microsoft.EntityFrameworkCore;

namespace ImoutoRebirth.Lilin.Infrastructure.Repositories
{
    public class FileNoteRepository : IFileNoteRepository
    {
        private readonly LilinDbContext _lilinDbContext;

        public FileNoteRepository(LilinDbContext lilinDbContext)
        {
            _lilinDbContext = lilinDbContext;
        }

        public async Task Add(FileNote fileNote)
        {
            await _lilinDbContext.Notes.AddAsync(fileNote.ToEntity());
        }

        public async Task<IReadOnlyCollection<FileNote>> GetForFile(Guid fileId)
        {
            var results = await _lilinDbContext.Notes
                                               .Where(x => x.FileId == fileId)
                                               .ToArrayAsync();

            return results.Select(x => x.ToModel()).ToArray();
        }

        public async Task Update(Guid noteId, Note note)
        {
            var entity = await _lilinDbContext.Notes.FindAsync(noteId);

            entity.Label = note.Label;
            entity.Height = note.Height;
            entity.Width = note.Width;
            entity.PositionFromTop = note.PositionFromTop;
            entity.PositionFromLeft = note.PositionFromLeft;
        }

        public async Task Delete(Guid noteId)
        {
            var note = await _lilinDbContext.Notes.FindAsync(noteId);

            _lilinDbContext.Remove(note);
        }
    }
}