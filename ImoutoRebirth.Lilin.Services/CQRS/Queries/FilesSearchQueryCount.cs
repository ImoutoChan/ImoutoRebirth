using System.Collections.Generic;
using ImoutoRebirth.Common.Cqrs.Abstract;
using ImoutoRebirth.Lilin.Core.Models;

namespace ImoutoRebirth.Lilin.Services.CQRS.Queries
{
    public class FilesSearchQueryCount : FilesSearchQueryBase, IQuery<uint>
    {
        public FilesSearchQueryCount(IReadOnlyCollection<TagSearchEntry> tagSearchEntries) 
            : base(tagSearchEntries)
        {
        }
    }
}