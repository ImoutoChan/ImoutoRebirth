using System;
using System.Collections.Generic;
using System.Linq;
using ImoutoRebirth.Common;

namespace ImoutoRebirth.Lilin.Core.Models
{
    public class MetadataUpdate
    {
        public Guid FileId { get; }

        public IReadOnlyCollection<FileTag> Tags { get; }

        public IReadOnlyCollection<FileNote> Notes { get; }

        public MetadataUpdate(Guid fileId, IReadOnlyCollection<FileTag> tags, IReadOnlyCollection<FileNote> notes)
        {
            ArgumentValidator.NotNull(() => tags, () => notes);
            ArgumentValidator.Requires(() => tags.All(t => t.FileId == fileId), nameof(tags));
            ArgumentValidator.Requires(() => notes.All(n => n.FileId == fileId), nameof(notes));

            FileId = fileId;
            Tags = tags;
            Notes = notes;
        }
    }
}