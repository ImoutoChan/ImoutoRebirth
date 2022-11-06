using ImoutoRebirth.Common.Cqrs.Abstract;
using ImoutoRebirth.Lilin.Core.Models;

namespace ImoutoRebirth.Lilin.Services.CQRS.Queries;

public class FilesFilterQuery : IQuery<Guid[]>
{
    public IReadOnlyCollection<Guid> FileIds { get; }

    public IReadOnlyCollection<TagSearchEntry> TagSearchEntries { get; }

    public FilesFilterQuery(IReadOnlyCollection<Guid> fileIds, IReadOnlyCollection<TagSearchEntry> tagSearchEntries)
    {
        FileIds = fileIds;
        TagSearchEntries = tagSearchEntries;
    }
}
