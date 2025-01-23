using ImoutoRebirth.Common;

namespace ImoutoRebirth.Arachne.Core.Models;

public class Note
{
    public string SourceId { get; }

    public string Label { get; }

    public NotePosition Position { get; }

    public Note(string label, NotePosition position, string sourceId)
    {
        ArgumentValidator.NotNull(() => label);

        Label = label;
        Position = position;
        SourceId = sourceId;
    }
}
