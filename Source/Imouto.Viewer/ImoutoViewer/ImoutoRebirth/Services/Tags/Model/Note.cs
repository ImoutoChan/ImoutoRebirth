namespace ImoutoViewer.ImoutoRebirth.Services.Tags.Model;

internal record Note(
    Guid Id,
    int PositionFromLeft,
    int PositionFromTop,
    int Width,
    int Height,
    string Label);
