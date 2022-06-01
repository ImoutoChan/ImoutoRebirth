namespace ImoutoViewer.Model;

internal class NoteM
{
    public NoteM(Guid id, string noteString, int positionX, int positionY, int width, int height)
    {
        Id = id;
        NoteString = noteString;
        PositionX = positionX;
        PositionY = positionY;
        Width = width;
        Height = height;
    }

    public Guid Id { get; }

    public string NoteString { get; }

    public int PositionX { get; }
        
    public int PositionY { get; }
        
    public int Width { get; }
        
    public int Height { get; }
}
