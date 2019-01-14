﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ImoutoRebirth.Lilin.Core.Models;

namespace ImoutoRebirth.Lilin.Core.Infrastructure
{
    public interface IFileNoteRepository : IRepository
    {
        Task Add(FileNote fileNote);

        Task<IReadOnlyCollection<FileNote>> GetForFile(Guid fileId);
    }
}