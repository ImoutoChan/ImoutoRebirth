namespace ImoutoRebirth.Arachne.Core.Models;

public class LoadedNotesHistory
{
    public LoadedNotesHistory(string[] changedPostIds, DateTimeOffset lastNoteUpdateDate)
    {
        ChangedPostIds = changedPostIds;
        LastNoteUpdateDate = lastNoteUpdateDate;
    }

    public string[] ChangedPostIds { get; }

    public DateTimeOffset LastNoteUpdateDate { get; }

    public void Deconstruct(out string[] changedPostIds, out DateTimeOffset lastNoteUpdateDate)
    {
        changedPostIds = ChangedPostIds;
        lastNoteUpdateDate = LastNoteUpdateDate;
    }
}
