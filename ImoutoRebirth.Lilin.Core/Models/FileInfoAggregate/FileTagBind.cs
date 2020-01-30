using System;
using ImoutoRebirth.Common;

namespace ImoutoRebirth.Lilin.Core.Models.FileInfoAggregate
{
    public class FileTagBind
    {
        public Guid FileId { get; }

        public string Type { get; }

        public string Name { get; }

        public string Value { get; }

        public string[] Synonyms { get; }

        public MetadataSource Source { get; }

        public FileTagBind(
            Guid fileId,
            MetadataSource source,
            string type,
            string name,
            string value,
            string[] synonyms)
        {
            ArgumentValidator.NotNull(() => type, () => name, () => synonyms);

            FileId = fileId;
            Value = value;
            Source = source;
            Type = type;
            Name = name;
            Synonyms = synonyms;
        }
    }
}