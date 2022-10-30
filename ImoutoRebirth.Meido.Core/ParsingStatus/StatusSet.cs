namespace ImoutoRebirth.Meido.Core.ParsingStatus;

public static class StatusSet
{
    public static IReadOnlyCollection<Status> AllFaulted => new []
    {
        Status.SearchRequested,
        Status.SearchFound,
        Status.SearchFailed,
        Status.OriginalRequested,
        Status.UpdateRequested
    };
}