using System.Collections.Generic;
using ImoutoRebirth.Lilin.Core.Models;
using ImoutoRebirth.Lilin.Services.CQRS.Abstract;

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