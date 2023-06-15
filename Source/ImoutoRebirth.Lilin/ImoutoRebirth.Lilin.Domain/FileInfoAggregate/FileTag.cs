using ImoutoRebirth.Common;

namespace ImoutoRebirth.Lilin.Domain.FileInfoAggregate;

public class FileTag : IEquatable<FileTag>
{
    public Guid FileId { get; }

    public Guid TagId { get; }

    public string? Value { get; private set; }

    public MetadataSource Source { get; }

    public FileTag(Guid fileId, Guid tagId, string? value, MetadataSource source)
    {
        value = value.TrimToNull();

        FileId = fileId;
        TagId = tagId;
        Value = value;
        Source = source;
    }

    public void UpdateValue(string? newValue)
    {
        Value = newValue;
    }

    public bool IsSameIdentity(FileTag tag)
        => FileId == tag.FileId
           && TagId == tag.TagId
           && Source == tag.Source;
        
    public bool Equals(FileTag? other)
    {
        if (other is null) 
            return false;

        if (ReferenceEquals(this, other)) 
            return true;

        return FileId == other.FileId
               && TagId == other.TagId
               && Source == other.Source
               && Value == other.Value;
    }

    public override bool Equals(object? obj)
    {
        if (obj is null)
            return false;

        if (ReferenceEquals(this, obj)) 
            return true;

        return obj.GetType() == GetType() && Equals((FileTag) obj);
    }

    public override int GetHashCode() 
        => HashCode.Combine(FileId, TagId, Value, (int) Source);
}
