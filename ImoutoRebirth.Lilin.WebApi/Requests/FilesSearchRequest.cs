using System.ComponentModel.DataAnnotations;

namespace ImoutoRebirth.Lilin.WebApi.Requests
{
    public class FilesSearchRequest
    {
        [Required]
        public IReadOnlyCollection<TagSearchEntryRequest> TagSearchEntries { get; set; } = default!;

        public int? Count { get; set; }

        public int? Skip { get; set; }
    }
}