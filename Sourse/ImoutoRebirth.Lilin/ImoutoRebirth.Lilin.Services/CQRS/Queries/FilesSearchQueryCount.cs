using ImoutoRebirth.Common.Cqrs.Abstract;
using ImoutoRebirth.Lilin.Core.Models;

namespace ImoutoRebirth.Lilin.Services.CQRS.Queries;

public class FilesSearchQueryCount : FilesSearchQueryBase, IQuery<int>
{
    public FilesSearchQueryCount(IReadOnlyCollection<TagSearchEntry> tagSearchEntries) 
        : base(tagSearchEntries)
    {
    }
}