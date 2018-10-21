namespace ImoutoRebirth.Arachne.Core.Models
{
    public readonly struct NotePosition
    {
        public readonly int PointTop;

        public readonly int PointLeft;

        public readonly int SizeWidth;

        public readonly int SizeHeight;

        public NotePosition(
            int pointTop,
            int pointLeft,
            int sizeWidth,
            int sizeHeight)
        {
            PointTop = pointTop;
            PointLeft = pointLeft;
            SizeWidth = sizeWidth;
            SizeHeight = sizeHeight;
        }
    }
}