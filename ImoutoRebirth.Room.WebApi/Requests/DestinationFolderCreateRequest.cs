using System.ComponentModel.DataAnnotations;

namespace ImoutoRebirth.Room.WebApi.Requests
{
    public class DestinationFolderCreateRequest
    {
        [Required]
        public string Path { get; set; }

        public bool ShouldCreateSubfoldersByHash { get; set; }

        public bool ShouldRenameByHash { get; set; }

        [Required]
        public string FormatErrorSubfolder { get; set; }

        [Required]
        public string HashErrorSubfolder { get; set; }

        [Required]
        public string WithoutHashErrorSubfolder { get; set; }
    }
}