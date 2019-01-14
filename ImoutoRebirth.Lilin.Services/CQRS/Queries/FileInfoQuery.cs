using System;
using ImoutoRebirth.Lilin.Core.Models;
using ImoutoRebirth.Lilin.Services.CQRS.Abstract;

namespace ImoutoRebirth.Lilin.Services.CQRS.Queries
{
    public class FileInfoQuery : IQuery<FileInfo>
    {
        public Guid FileId { get; }

        public FileInfoQuery(Guid fileId)
        {
            FileId = fileId;
        }
    }
}