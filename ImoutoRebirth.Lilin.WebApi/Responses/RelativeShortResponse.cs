using ImoutoRebirth.Lilin.Core.Models;

namespace ImoutoRebirth.Lilin.WebApi.Responses;

public class RelativeShortResponse
{
    /// <summary>
    ///     Requested md5 hash.
    /// </summary>
    public string Hash { get; }
    
    /// <summary>
    ///     Type of relative with md5 from request.
    /// </summary>
    public RelativeType? RelativeType { get; }

    public RelativeShortResponse(string hash, RelativeType relativeType)
    {
        Hash = hash;
        RelativeType = relativeType;
    }
}
