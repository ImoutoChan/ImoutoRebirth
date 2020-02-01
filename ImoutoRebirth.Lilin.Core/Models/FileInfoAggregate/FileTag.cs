using System;
using ImoutoRebirth.Common;

namespace ImoutoRebirth.Lilin.Core.Models.FileInfoAggregate
{
    public class FileTag
    {
        public Guid FileId { get; }

        public Tag Tag { get; }

        public string? Value { get; }

        public MetadataSource Source { get; }

        public FileTag(Guid fileId, Tag tag, string? value, MetadataSource source)
        {
            ArgumentValidator.NotNull(() => tag);
            ArgumentValidator.Requires(() => tag.HasValue || value == null, nameof(value));

            FileId = fileId;
            Tag = tag;
            Value = value;
            Source = source;
        }

        public bool IsSameIdentity(FileTag tag) 
            => this.FileId == tag.FileId 
               && this.Tag.Id == tag.Tag.Id 
               && this.Source == tag.Source;
    }
}