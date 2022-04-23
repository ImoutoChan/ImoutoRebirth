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
    public RelativeType? RelativesType { get; }

    public RelativeShortResponse(string hash, RelativeType relativesType)
    {
        Hash = hash;
        RelativesType = relativesType;
    }
}