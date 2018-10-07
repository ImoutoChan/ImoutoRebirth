using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ImoutoRebirth.Room.Database.Entities.Abstract;

namespace ImoutoRebirth.Room.Database.Entities
{
    public class SourceFolderEntity : EntityBase
    {
        private const string _separator = ";";
        private string _supportedExtensions;

        [ForeignKey(nameof(Collection))]
        public Guid CollectionId { get; set; }

        [Required]
        public string Path { get; set; }

        public bool ShouldCheckFormat { get; set; }

        public bool ShouldCheckHashFromName { get; set; }

        public bool ShouldCreateTagsFromSubfolders { get; set; }

        public bool ShouldAddTagFromFilename { get; set; }

        [NotMapped]
        public IReadOnlyCollection<string> SupportedExtensionCollection
        {
            get => _supportedExtensions?.Split(new [] {_separator}, StringSplitOptions.RemoveEmptyEntries);
            set => _supportedExtensions = value != null ? string.Join(_separator, value) : null;
        }

        public CollectionEntity Collection { get; set; }
    }
}