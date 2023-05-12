namespace ImoutoRebirth.Lilin.Core.Models;

public class RelativeShortInfo
{
    public string Hash { get; }

    public RelativeType? RelativeType { get; }

    public RelativeShortInfo(string hash, RelativeType? relativeType)
    {
        Hash = hash;
        RelativeType = relativeType;
    }
}