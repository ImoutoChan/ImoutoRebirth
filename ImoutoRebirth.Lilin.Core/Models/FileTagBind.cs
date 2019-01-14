using System;

namespace ImoutoRebirth.Lilin.Core.Models
{
    public class FileTagBind
    {
        public Guid FileId { get; }

        public Guid TagId { get; }

        public string Value { get; }

        public MetadataSource Source { get; }

        public FileTagBind(Guid fileId, Guid tagId, string value, MetadataSource source)
        {
            FileId = fileId;
            TagId = tagId;
            Value = value;
            Source = source;
        }
    }
}