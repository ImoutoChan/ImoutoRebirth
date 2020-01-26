using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ImoutoRebirth.Room.Database.Entities.Abstract;

namespace ImoutoRebirth.Room.Database.Entities
{
    public class SourceFolderEntity : EntityBase
    {
        private const string SupportedExtensionsSeparator = ";";

        // ReSharper disable once InconsistentNaming
        // Rename after ef core 3.0 release. Before that backing fields should be named as properties.
        private string SupportedExtensions;

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
            get => SupportedExtensions
                ?.Split(new[] {SupportedExtensionsSeparator}, StringSplitOptions.RemoveEmptyEntries);
            set => SupportedExtensions = value != null ? string.Join(SupportedExtensionsSeparator, value) : null;
        }

        public CollectionEntity Collection { get; set; }
    }
}