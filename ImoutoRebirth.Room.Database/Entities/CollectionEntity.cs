using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using ImoutoRebirth.Room.Database.Entities.Abstract;

namespace ImoutoRebirth.Room.Database.Entities
{
    public class CollectionEntity : EntityBase
    {
        [Required]
        public string Name { get; set; } = default!;

        public DestinationFolderEntity? DestinationFolder { get; set; }

        public IList<SourceFolderEntity> SourceFolders { get; set; } = default!;

        public IList<CollectionFileEntity> Files { get; set; } = default!;
    }
}
