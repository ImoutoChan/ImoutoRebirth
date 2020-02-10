using System.Collections.Generic;

namespace ImoutoRebirth.Lilin.WebApi.Requests
{
    public class FilesSearchRequest
    {
        public IReadOnlyCollection<TagSearchEntryRequest> TagSearchEntries { get; set; }

        public int? Count { get; set; }

        public int? Skip { get; set; }
    }
}