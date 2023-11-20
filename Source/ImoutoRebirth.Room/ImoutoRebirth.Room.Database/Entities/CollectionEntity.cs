using System.ComponentModel.DataAnnotations;
using ImoutoRebirth.Room.Database.Entities.Abstract;

namespace ImoutoRebirth.Room.Database.Entities;

public class CollectionEntity : EntityBase
{
    [Required]
    public required string Name { get; set; }

    public DestinationFolderEntity? DestinationFolder { get; set; }

    public IList<SourceFolderEntity>? SourceFolders { get; set; }

    public IList<CollectionFileEntity>? Files { get; set; }
}
