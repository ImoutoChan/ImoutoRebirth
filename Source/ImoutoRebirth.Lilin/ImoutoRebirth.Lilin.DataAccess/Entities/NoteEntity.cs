using System.ComponentModel.DataAnnotations;
using ImoutoRebirth.Lilin.Core.FileInfoAggregate;

namespace ImoutoRebirth.Lilin.DataAccess.Entities;

public class NoteEntity : EntityBase
{
    public Guid FileId { get; set; }

    public MetadataSource Source { get; set; }

    public int? SourceId { get; set; }

    [Required]
    public string Label { get; set; } = default!;

    public int PositionFromLeft { get; set; }

    public int PositionFromTop { get; set; }

    public int Width { get; set; }

    public int Height { get; set; }
}
