namespace ImoutoViewer.Model;

class NoteM
{
    private readonly string _noteString;

    public NoteM(int? id, string noteString, int positionX, int positionY, int width, int height)
    {
        Id = id;
        _noteString = noteString;
        PositionX = positionX;
        PositionY = positionY;
        Width = width;
        Height = height;
    }

    public int? Id { get; }

    public string NoteString
    {
        get { return _noteString; }
    }

    public int PositionX { get; }
        
    public int PositionY { get; }
        
    public int Width { get; }
        
    public int Height { get; }
}