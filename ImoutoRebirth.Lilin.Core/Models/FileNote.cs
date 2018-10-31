using System;
using ImoutoRebirth.Common;

namespace ImoutoRebirth.Lilin.Core.Models
{
    public class FileNote
    {
        public Guid FileId { get; }

        public Note Note { get; }

        public MetadataSource Source { get; }

        public int? SourceId { get; }

        public FileNote(Guid fileId, Note note, MetadataSource source, int? sourceId)
        {
            ArgumentValidator.NotNull(() => note);

            FileId = fileId;
            Note = note;
            Source = source;
            SourceId = sourceId;
        }
    }
}