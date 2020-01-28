using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ImoutoRebirth.Lilin.DataAccess.Entities
{
    public class TagTypeEntity : EntityBase
    {
        [Required]
        public string Name { get; set; } = default!;

        // todo change default
        public int Color { get; set; } = 16741916;


        public IReadOnlyCollection<TagEntity>? Tags { get; set; }
    }
}