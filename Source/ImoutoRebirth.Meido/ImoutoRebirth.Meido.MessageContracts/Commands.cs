namespace ImoutoRebirth.Meido.MessageContracts;

public record SearchCompleteCommand(
    Guid FileId,
    int SourceId,
    SearchStatus ResultStatus,
    string? ErrorText = null,
    string? FileIdFromSource = null);

public enum SearchStatus
{
    NotFound = 0,
    Success = 1,
    Error = 2
}

public record TagsUpdatedCommand(int SourceId, string[] PostIds, int LastHistoryId);

public record SavedCommand(Guid FileId, int SourceId);

public record NotesUpdatedCommand(int SourceId, string[] PostIds, DateTimeOffset LastNoteUpdateDate);

public record NewFileCommand(string Md5, Guid FileId, string FileName);

public record ProcessRenamedFileCommand(string Md5, Guid FileId, string NewFileName);



