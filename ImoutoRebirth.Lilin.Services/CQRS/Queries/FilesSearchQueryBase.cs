using System.Collections.Generic;
using ImoutoRebirth.Common;
using ImoutoRebirth.Lilin.Core.Models;

namespace ImoutoRebirth.Lilin.Services.CQRS.Queries
{
    public class FilesSearchQueryBase
    {
        public IReadOnlyCollection<TagSearchEntry> TagSearchEntries { get; }

        public FilesSearchQueryBase(IReadOnlyCollection<TagSearchEntry> tagSearchEntries)
        {
            ArgumentValidator.NotNull(() => tagSearchEntries);

            TagSearchEntries = tagSearchEntries;
        }
    }
}