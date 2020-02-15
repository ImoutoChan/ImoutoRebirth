using System;

namespace ImoutoRebirth.Navigator.Services.Tags
{
    internal class FileTag
    {
        public FileTag(Guid fileId, Tag tag, string? value, FileTagSource source)
        {
            FileId = fileId;
            Value = value;
            Source = source;
            Tag = tag;
        }

        public Guid FileId { get; }

        public Tag Tag { get; }

        public string? Value { get; set; }

        public FileTagSource Source { get; }

        public bool IsEditable => Source == FileTagSource.Manual;
    }
}