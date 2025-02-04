namespace ImoutoRebirth.Arachne.MessageContracts;

public enum SearchEngineType : byte
{
    Yandere = 0,
    Danbooru = 1,
    Sankaku = 2,
    Gelbooru = 4,
    Rule34 = 5,
    ExHentai = 6
}

public abstract record SearchMetadataCommand(string Md5, Guid FileId, string FileName);

public record EverywhereSearchMetadataCommand(string Md5, Guid FileId, string FileName)
    : SearchMetadataCommand(Md5, FileId, FileName);

public record YandereSearchMetadataCommand(string Md5, Guid FileId, string FileName)
    : SearchMetadataCommand(Md5, FileId, FileName);

public record DanbooruSearchMetadataCommand(string Md5, Guid FileId, string FileName)
    : SearchMetadataCommand(Md5, FileId, FileName);

public record SankakuSearchMetadataCommand(string Md5, Guid FileId, string FileName)
    : SearchMetadataCommand(Md5, FileId, FileName);

public record GelbooruSearchMetadataCommand(string Md5, Guid FileId, string FileName)
    : SearchMetadataCommand(Md5, FileId, FileName);

public record Rule34SearchMetadataCommand(string Md5, Guid FileId, string FileName)
    : SearchMetadataCommand(Md5, FileId, FileName);

public record ExHentaiSearchMetadataCommand(string Md5, Guid FileId, string FileName)
    : SearchMetadataCommand(Md5, FileId, FileName);

public record LoadNoteHistoryCommand(SearchEngineType SearchEngineType, DateTimeOffset LastProcessedNoteUpdateAt);

public record LoadTagHistoryCommand(SearchEngineType SearchEngineType, int LastProcessedTagHistoryId);
