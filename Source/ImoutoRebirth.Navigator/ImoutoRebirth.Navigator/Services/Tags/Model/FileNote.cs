namespace ImoutoRebirth.Navigator.Services.Tags.Model;

internal record FileNote(
    Guid FileId,
    int Height,
    int Width,
    string Label,
    int PositionFromLeft,
    int PositionFromTop,
    FileTagSource Source,
    int? SourceId);
