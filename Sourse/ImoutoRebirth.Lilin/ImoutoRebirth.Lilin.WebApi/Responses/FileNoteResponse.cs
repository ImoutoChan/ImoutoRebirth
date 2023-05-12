using ImoutoRebirth.Lilin.Core.Models;

namespace ImoutoRebirth.Lilin.WebApi.Responses;

public class FileNoteResponse
{
    public Guid FileId { get; }

    public NoteResponse Note { get; }

    public MetadataSource Source { get; }

    public int? SourceId { get; }

    public FileNoteResponse(Guid fileId, NoteResponse note, MetadataSource source, int? sourceId)
    {
        FileId = fileId;
        Note = note;
        Source = source;
        SourceId = sourceId;
    }
}