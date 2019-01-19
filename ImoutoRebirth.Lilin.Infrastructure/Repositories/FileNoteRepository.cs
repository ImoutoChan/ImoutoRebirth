using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ImoutoRebirth.Lilin.Core.Infrastructure;
using ImoutoRebirth.Lilin.Core.Models;
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

        public IUnitOfWork UnitOfWork => _lilinDbContext;

        public async Task Add(FileNote fileNote)
        {
            await _lilinDbContext.Notes.AddAsync(fileNote.ToEntity());
        }

        public async Task<IReadOnlyCollection<FileNote>> GetForFile(Guid fileId)
        {
            var results = await _lilinDbContext.Notes.Where(x => x.FileId == fileId).ToArrayAsync();
            return results.Select(x => x.ToModel()).ToArray();
        }
    }
}