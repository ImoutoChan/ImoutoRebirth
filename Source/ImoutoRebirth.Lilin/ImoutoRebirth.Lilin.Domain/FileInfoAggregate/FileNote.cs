using ImoutoRebirth.Common;

namespace ImoutoRebirth.Lilin.Domain.FileInfoAggregate;

public class FileNote
{
    public Guid FileId { get; }

    public string Label { get; }

    public int PositionFromLeft { get; }

    public int PositionFromTop { get; }

    public int Width { get; }

    public int Height { get; }

    public MetadataSource Source { get; }

    /// <summary>
    /// External id on source booru site.
    /// </summary>
    public string? SourceId { get; }

    public FileNote(
        Guid fileId,
        string label,
        int positionFromLeft,
        int positionFromTop,
        int width,
        int height,
        MetadataSource source,
        string? sourceId)
    {
        ArgumentValidator.NotNull(() => label);

        FileId = fileId;
        Source = source;
        SourceId = sourceId;
        Label = label;
        PositionFromLeft = positionFromLeft;
        PositionFromTop = positionFromTop;
        Width = width;
        Height = height;
    }

    public bool IsSameIdentity(FileNote note) =>
        FileId == note.FileId
        && Source == note.Source
        && SourceId == note.SourceId;
    
    public (Guid FileId, MetadataSource Source, string? SourceId) GetIdentity() => (FileId, Source, SourceId);

    public bool IsSameContent(FileNote note) =>
        Label == note.Label
        && PositionFromLeft == note.PositionFromLeft
        && PositionFromTop == note.PositionFromTop
        && Width == note.Width
        && Height == note.Height;

    public static FileNote Create(
        Guid fileId,
        string label,
        int positionFromLeft,
        int positionFromTop,
        int width,
        int height,
        MetadataSource source,
        string? sourceId)
        => new(
            fileId,
            label, 
            positionFromLeft, 
            positionFromTop, 
            width, 
            height,
            source,
            sourceId);
}
