using System.ComponentModel.DataAnnotations;

namespace ImoutoRebirth.Lilin.DataAccess.Entities;

public class TagTypeEntity : EntityBase
{
    [Required]
    public required string Name { get; set; }

    public int Color { get; set; } = 16741916;

    public IReadOnlyCollection<TagEntity>? Tags { get; set; }
}
