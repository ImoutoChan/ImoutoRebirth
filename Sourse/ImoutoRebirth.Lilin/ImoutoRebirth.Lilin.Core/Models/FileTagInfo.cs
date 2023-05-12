namespace ImoutoRebirth.Lilin.Core.Models;

public class FileTagInfo
{
    public Guid FileId { get; }

    public Guid TagId { get; }

    public string? Value { get; }

    public MetadataSource Source { get; }

    public FileTagInfo(Guid fileId, Guid tagId, string? value, MetadataSource source)
    {
        if (string.IsNullOrWhiteSpace(value))
            value = null;

        FileId = fileId;
        TagId = tagId;
        Value = value;
        Source = source;
    }
}