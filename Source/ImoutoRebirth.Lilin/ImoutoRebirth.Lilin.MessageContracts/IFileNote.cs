namespace ImoutoRebirth.Lilin.MessageContracts;

public interface IFileNote
{
    string? SourceId { get; }

    string Label { get; }

    int PositionFromLeft { get; }

    int PositionFromTop { get; }

    int Width { get; }

    int Height { get; }
}
