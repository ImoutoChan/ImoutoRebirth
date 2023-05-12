namespace ImoutoRebirth.Arachne.Core.Models;

public class LoadedTagsHistory
{
    public LoadedTagsHistory(IReadOnlyCollection<int> changedPostIds, int lastHistoryId)
    {
        ChangedPostIds = changedPostIds;
        LastHistoryId = lastHistoryId;
    }

    public IReadOnlyCollection<int> ChangedPostIds { get; }

    public int LastHistoryId { get; }

    public void Deconstruct(out IReadOnlyCollection<int> changedPostIds, out int lastHistoryId)
    {
        changedPostIds = ChangedPostIds;
        lastHistoryId = LastHistoryId;
    }
}
