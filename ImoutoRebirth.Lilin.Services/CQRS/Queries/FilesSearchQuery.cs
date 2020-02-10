using System;
using System.Collections.Generic;
using ImoutoRebirth.Common.Cqrs.Abstract;
using ImoutoRebirth.Lilin.Core.Models;

namespace ImoutoRebirth.Lilin.Services.CQRS.Queries
{
    public class FilesSearchQuery : FilesSearchQueryBase, IQuery<Guid[]>
    {
        public int? Limit { get; }

        public int Offset { get; }

        public FilesSearchQuery(
            IReadOnlyCollection<TagSearchEntry> tagSearchEntries,
            int? limit = null,
            int offset = 0)
            : base(tagSearchEntries)
        {
            Limit = limit;
            Offset = offset;
        }
    }
}
