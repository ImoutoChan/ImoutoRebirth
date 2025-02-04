namespace ImoutoRebirth.Lilin.MessageContracts;

public enum MetadataSource
{
    Yandere = 0,
    Danbooru = 1,
    Sankaku = 2,
    Manual = 3,
    Gelbooru = 4,
    Rule34 = 5,
    ExHentai = 6
}

public record UpdateMetadataCommand(
    Guid FileId,
    MetadataSource MetadataSource,
    FileNote[] FileNotes,
    FileTag[] FileTags);

public record FileTag(string Type, string Name, string? Value, string[]? Synonyms);

public record FileNote(
    string? SourceId,
    string Label,
    int PositionFromLeft,
    int PositionFromTop,
    int Width,
    int Height);
