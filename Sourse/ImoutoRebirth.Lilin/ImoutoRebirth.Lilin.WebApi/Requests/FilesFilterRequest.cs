using System.ComponentModel.DataAnnotations;

namespace ImoutoRebirth.Lilin.WebApi.Requests;

public class FilesFilterRequest
{
    [Required]
    public IReadOnlyCollection<Guid> FileIds { get; set; } = default!;

    [Required]
    public IReadOnlyCollection<TagSearchEntryRequest> TagSearchEntries { get; set; } = default!;
}