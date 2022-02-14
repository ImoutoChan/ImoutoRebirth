using ImoutoRebirth.Common;

namespace ImoutoRebirth.Arachne.Core.Models;

public class Note
{
    public int SourceId { get; }

    public string Label { get; }

    public NotePosition Position { get; }

    public Note(string label, NotePosition position, int sourceId)
    {
        ArgumentValidator.NotNull(() => label);

        Label = label;
        Position = position;
        SourceId = sourceId;
    }
}