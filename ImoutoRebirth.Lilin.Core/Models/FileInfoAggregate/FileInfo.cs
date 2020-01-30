using System;
using System.Collections.Generic;
using ImoutoRebirth.Lilin.Core.Events;

namespace ImoutoRebirth.Lilin.Core.Models.FileInfoAggregate
{
    public class FileInfo
    {
        private List<FileTag> _tags;
        private List<FileNote> _notes;

        public IReadOnlyCollection<FileTag> Tags => _tags;

        public IReadOnlyCollection<FileNote> Notes => _notes;

        public FileInfo(IEnumerable<FileTag> tags, IEnumerable<FileNote> notes)
        {
            _tags = new List<FileTag>(tags);
            _notes = new List<FileNote>(notes);
        }

        public DomainResult UpdateMetadata(MetadataUpdateData updateData)
        {
            var result = new DomainResult();

            if (updateData.MetadataSource != MetadataSource.Manual)
            {
                _tags = new List<FileTag>();
                _notes = new List<FileNote>();
            }

            foreach (var tag in updateData.Tags)
            {
                _tags.Add(tag);
            }

            foreach (var note in updateData.Notes)
            {
                _notes.Add(note);
            }

            result.Add(new MetadataUpdated(updateData.FileId, updateData.MetadataSource));

            return result;
        }
    }
}