using System;
using ImoutoRebirth.Common;

namespace ImoutoRebirth.Lilin.Core.Models.FileInfoAggregate
{
    public class FileTag : IEquatable<FileTag>
    {
        public Guid FileId { get; }

        public Tag Tag { get; }

        public string? Value { get; private set; }

        public MetadataSource Source { get; }

        public FileTag(Guid fileId, Tag tag, string? value, MetadataSource source)
        {
            ArgumentValidator.NotNull(() => tag);
            
            if (string.IsNullOrWhiteSpace(value))
                value = null;

            FileId = fileId;
            Tag = tag;
            Value = value;
            Source = source;
        }

        public void UpdateValue(string? newValue)
        {
            Value = newValue;
        }

        public bool IsSameIdentity(FileTag tag)
            => this.FileId == tag.FileId
               && this.Tag.Id == tag.Tag.Id
               && this.Source == tag.Source;
        
        public bool Equals(FileTag? other)
        {
            if (other is null) 
                return false;

            if (ReferenceEquals(this, other)) 
                return true;

            return FileId.Equals(other.FileId)
                   && Tag.Id.Equals(other.Tag.Id)
                   && Source == other.Source
                   && Value == other.Value;
        }

        public override bool Equals(object? obj)
        {
            if (obj is null) 
                return false;

            if (ReferenceEquals(this, obj)) 
                return true;

            return obj.GetType() == this.GetType() 
                   && Equals((FileTag) obj);
        }

        public override int GetHashCode() 
            => HashCode.Combine(FileId, Tag.Id, Value, (int) Source);
    }
}