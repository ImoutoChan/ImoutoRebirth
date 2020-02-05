using System;
using System.Collections.Generic;
using ImoutoRebirth.Common.Cqrs.Abstract;
using ImoutoRebirth.Lilin.Core.Models;

namespace ImoutoRebirth.Lilin.Services.CQRS.Queries
{
    public class FilesSearchQuery : FilesSearchQueryBase, IQuery<Guid[]>
    {
        public uint? Limit { get; }

        public uint Offset { get; }

        public FilesSearchQuery(
            IReadOnlyCollection<TagSearchEntry> tagSearchEntries, 
            uint? limit = null, 
            uint offset = 0)
            : base(tagSearchEntries)
        {
            Limit = limit;
            Offset = offset;
        }
    }
}
