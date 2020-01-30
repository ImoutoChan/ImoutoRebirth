using System;
using System.Collections.Generic;
using ImoutoProject.Common.Cqrs.Abstract;
using ImoutoRebirth.Lilin.Core.Models;
using ImoutoRebirth.Lilin.Core.Models.FileInfoAggregate;

namespace ImoutoRebirth.Lilin.Services.CQRS.Queries
{
    public class FilesSearchQuery : FilesSearchQueryBase, IQuery<Guid[]>, IQuery<FileInfo>
    {
        public uint? Limit { get; }

        public uint Offset { get; }

        public FilesSearchQuery(
            IReadOnlyCollection<TagSearchEntry> tagSearchEntries, 
            uint? limit, 
            uint offset = 0)
            : base(tagSearchEntries)
        {
            Limit = limit;
            Offset = offset;
        }
    }
}
