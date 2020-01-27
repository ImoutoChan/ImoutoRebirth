using System;

namespace ImoutoRebirth.Lilin.WebApi.Responses
{
    public class NoteResponse
    {
        public Guid Id { get; }

        public string Label { get; }

        public int PositionFromLeft { get; }

        public int PositionFromTop { get; }

        public int Width { get; }

        public int Height { get; }

        public NoteResponse(Guid id, string label, int positionFromLeft, int positionFromTop, int width, int height)
        {
            Id = id;
            Label = label;
            PositionFromLeft = positionFromLeft;
            PositionFromTop = positionFromTop;
            Width = width;
            Height = height;
        }
    }
}