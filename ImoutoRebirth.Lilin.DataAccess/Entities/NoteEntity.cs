using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ImoutoRebirth.Lilin.Core.Models;

namespace ImoutoRebirth.Lilin.DataAccess.Entities
{
    public class NoteEntity
    {
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public Guid Id { get; set; }

        public Guid FileId { get; set; }

        public MetadataSource Source { get; set; }

        public int? SourceId { get; set; }

        [Required]
        public string Label { get; set; }

        public int PositionFromLeft { get; set; }

        public int PositionFromTop { get; set; }

        public int Width { get; set; }

        public int Height { get; set; }
    }
}