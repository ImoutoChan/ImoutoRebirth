using System.ComponentModel.DataAnnotations;

namespace ImoutoRebirth.Lilin.WebApi.Requests;

public class TagCreateRequest
{
    public Guid TypeId { get; set; }

    [Required]
    public string Name { get; set; } = default!;

    public bool HasValue { get; set; }

    public IReadOnlyCollection<string>? Synonyms { get; set; }
}