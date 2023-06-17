namespace ImoutoViewer.ImoutoRebirth.Services.Tags.Model;

internal record FileNote(
    Guid FileId,
    int Height,
    string? Label,
    int PositionFromLeft,
    int PositionFromTop,
    FileTagSource Source,
    int? SourceId,
    int Width);
