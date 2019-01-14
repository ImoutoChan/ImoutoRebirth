using System.Collections.Generic;

namespace ImoutoRebirth.Lilin.Core.Models
{
    public class FileInfo
    {
        public IReadOnlyCollection<FileTag> Tags { get; }

        public IReadOnlyCollection<FileNote> Notes { get; }

        public FileInfo(IReadOnlyCollection<FileTag> tags, IReadOnlyCollection<FileNote> notes)
        {
            Tags = tags;
            Notes = notes;
        }
    }
}