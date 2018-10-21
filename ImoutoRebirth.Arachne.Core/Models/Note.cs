using ImoutoRebirth.Common;

namespace ImoutoRebirth.Arachne.Core.Models
{
    public class Note
    {
        public string Label { get; }

        public NotePosition Position { get; }

        public Note(string label, NotePosition position)
        {
            ArgumentValidator.NotNull(() => label);

            Label = label;
            Position = position;
        }
    }
}