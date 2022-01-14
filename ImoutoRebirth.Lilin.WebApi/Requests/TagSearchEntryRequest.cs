using ImoutoRebirth.Lilin.Core.Models;

namespace ImoutoRebirth.Lilin.WebApi.Requests;

public class TagSearchEntryRequest
{
    public Guid TagId { get; set; }

    public string? Value { get; set; }

    public TagSearchScope TagSearchScope { get; set; }
}