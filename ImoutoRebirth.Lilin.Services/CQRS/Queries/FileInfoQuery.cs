using ImoutoRebirth.Common.Cqrs.Abstract;
using ImoutoRebirth.Lilin.Core.Models.FileInfoAggregate;
using System;

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