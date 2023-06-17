using ImoutoViewer.ImoutoRebirth.Services.Tags.Model;

namespace ImoutoViewer.Model;

internal class NoteM
{
    public NoteM(string noteString, int positionX, int positionY, int width, int height, FileTagSource source)
    {
        NoteString = noteString;
        PositionX = positionX;
        PositionY = positionY;
        Width = width;
        Height = height;
        Source = source;
    }

    public string NoteString { get; }

    public int PositionX { get; }
        
    public int PositionY { get; }
        
    public int Width { get; }
        
    public int Height { get; }
    
    public FileTagSource Source { get; }
}
