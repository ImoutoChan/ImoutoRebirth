namespace ImoutoRebirth.Meido.Domain.ParsingStatusAggregate;

public static class StatusSet
{
    public static IReadOnlyCollection<Status> AllFaulted =>
    [
        Status.SearchRequested,
        Status.SearchFound,
        Status.SearchFailed,
        Status.OriginalRequested,
        Status.UpdateRequested
    ];
}
