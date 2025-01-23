namespace ImoutoRebirth.Arachne.Core.Models;

public class LoadedTagsHistory
{
    public LoadedTagsHistory(IReadOnlyCollection<string> changedPostIds, int lastHistoryId)
    {
        ChangedPostIds = changedPostIds;
        LastHistoryId = lastHistoryId;
    }

    public IReadOnlyCollection<string> ChangedPostIds { get; }

    public int LastHistoryId { get; }

    public void Deconstruct(out IReadOnlyCollection<string> changedPostIds, out int lastHistoryId)
    {
        changedPostIds = ChangedPostIds;
        lastHistoryId = LastHistoryId;
    }
}
