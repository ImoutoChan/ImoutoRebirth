using ImoutoRebirth.Common;

namespace ImoutoRebirth.Lilin.Core.Models
{
    public class Note
    {
        public Guid Id { get; }

        public string Label { get; }

        public int PositionFromLeft { get; }

        public int PositionFromTop { get; }

        public int Width { get; }

        public int Height { get; }

        public Note(Guid id, string label, int positionFromLeft, int positionFromTop, int width, int height)
        {
            ArgumentValidator.NotNull(() => label);

            Id = id;
            Label = label;
            PositionFromLeft = positionFromLeft;
            PositionFromTop = positionFromTop;
            Width = width;
            Height = height;
        }

        public static Note CreateNew(string label, int positionFromLeft, int positionFromTop, int width, int height)
            => new Note(Guid.NewGuid(), label, positionFromLeft, positionFromTop, width, height);
    }
}
