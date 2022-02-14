namespace ImoutoRebirth.Arachne.Core.Models;

public class LoadedNotesHistory
{
    public LoadedNotesHistory(int[] changedPostIds, DateTimeOffset lastNoteUpdateDate)
    {
        ChangedPostIds = changedPostIds;
        LastNoteUpdateDate = lastNoteUpdateDate;
    }

    public int[] ChangedPostIds { get; }

    public DateTimeOffset LastNoteUpdateDate { get; }

    public void Deconstruct(out int[] changedPostIds, out DateTimeOffset lastNoteUpdateDate)
    {
        changedPostIds = ChangedPostIds;
        lastNoteUpdateDate = LastNoteUpdateDate;
    }
}