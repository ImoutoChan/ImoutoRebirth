using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ImoutoRebirth.Lilin.DataAccess.Entities
{
    public class TagTypeEntity : EntityBase
    {
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public Guid Id { get; set; }

        [Required]
        public string Name { get; set; }

        // todo change default
        public int Color { get; set; } = 16741916;

        public IReadOnlyCollection<TagEntity> Tags { get; set; }
    }
}