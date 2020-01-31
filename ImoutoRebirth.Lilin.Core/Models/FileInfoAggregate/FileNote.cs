using System;
using ImoutoRebirth.Common;

namespace ImoutoRebirth.Lilin.Core.Models.FileInfoAggregate
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

        public bool IsSameIdentity(FileNote note) =>
            this.FileId == note.FileId
            && this.Source == note.Source
            && this.SourceId == note.SourceId;
    }
}