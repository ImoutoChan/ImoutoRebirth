using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ImoutoRebirth.Room.Database.Entities.Abstract;

namespace ImoutoRebirth.Room.Database.Entities;

public class DestinationFolderEntity : EntityBase
{
    [ForeignKey(nameof(Collection))]
    public Guid CollectionId { get; set; }

    [Required]
    public required string Path { get; set; }

    public bool ShouldCreateSubfoldersByHash { get; set; }

    public bool ShouldRenameByHash { get; set; }

    [Required]
    public required string FormatErrorSubfolder { get; set; }

    [Required]
    public required string HashErrorSubfolder { get; set; }

    [Required]
    public required string WithoutHashErrorSubfolder { get; set; }

    public CollectionEntity? Collection { get; set; }
}
