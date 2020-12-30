using System;
using System.Collections.Generic;
using System.Linq;
using ImoutoRebirth.Lilin.Core.Events;

namespace ImoutoRebirth.Lilin.Core.Models.FileInfoAggregate
{
    public class FileInfo
    {
        private List<FileTag> _tags;
        private List<FileNote> _notes;

        public Guid Id { get; }

        public IReadOnlyCollection<FileTag> Tags => _tags;

        public IReadOnlyCollection<FileNote> Notes => _notes;

        public FileInfo(IEnumerable<FileTag> tags, IEnumerable<FileNote> notes, Guid id)
        {
            Id = id;
            _tags = new List<FileTag>(tags);
            _notes = new List<FileNote>(notes);
        }

        public DomainResult UpdateMetadata(MetadataUpdateData updateData)
        {
            var result = new DomainResult();

            if (updateData.MetadataSource != MetadataSource.Manual)
            {
                _tags = _tags.Where(x => x.Source != updateData.MetadataSource).ToList();
                _notes = _notes.Where(x => x.Source != updateData.MetadataSource).ToList();
            }

            foreach (var tag in updateData.Tags)
            {
                if (_tags.Any(x => x.Equals(tag)))
                    continue;
                
                _tags.Add(tag);
            }

            foreach (var note in updateData.Notes)
            {
                if (_notes.Any(x => x.IsSameIdentity(note)))
                    _notes = _notes.Where(x => !x.IsSameIdentity(note)).ToList();

                _notes.Add(note);
            }

            result.Add(new MetadataUpdated(updateData.FileId, updateData.MetadataSource));

            return result;
        }

        public void AddFileTag(FileTag newTag)
        {
            _tags.Add(newTag);
        }

        public void RemoveFileTag(Guid tagId, MetadataSource source, string? value)
        {
            var tag = Tags.FirstOrDefault(
                x => x.Tag.Id == tagId
                     && x.Source == source
                     && value == x.Value);

            if (tag != null)
                _tags.Remove(tag);
        }
    }
}