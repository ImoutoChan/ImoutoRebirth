namespace ImoutoRebirth.Arachne.Core.Models;

public class LoadedTagsHistory
{
    public LoadedTagsHistory(int[] changedPostIds, int lastHistoryId)
    {
        ChangedPostIds = changedPostIds;
        LastHistoryId = lastHistoryId;
    }

    public int[] ChangedPostIds { get; }

    public int LastHistoryId { get; }

    public void Deconstruct(out int[] changedPostIds, out int lastHistoryId)
    {
        changedPostIds = ChangedPostIds;
        lastHistoryId = LastHistoryId;
    }
}