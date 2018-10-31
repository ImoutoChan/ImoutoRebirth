using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ImoutoRebirth.Lilin.DataAccess.Entities
{
    public class TagEntity
    {
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public Guid Id { get; set; }

        public Guid TypeId { get; set; }

        [Required]
        public string Name { get; set; }

        public bool HasValue { get; set; }

        public string Synonyms { get; set; }

        public int Count { get; set; }

        public TagTypeEntity Type { get; set; }

        public IReadOnlyCollection<FileTagEntity> FileTags { get; set; }
    }
}