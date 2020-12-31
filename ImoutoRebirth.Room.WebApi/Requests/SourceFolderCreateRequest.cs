using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ImoutoRebirth.Room.WebApi.Requests
{
    public class SourceFolderCreateRequest
    {
        [Required]
        public string Path { get; set; } = default!;

        public bool ShouldCheckFormat { get; set; }

        public bool ShouldCheckHashFromName { get; set; }

        public bool ShouldCreateTagsFromSubfolders { get; set; }

        public bool ShouldAddTagFromFilename { get; set; }

        public IReadOnlyCollection<string> SupportedExtensions { get; set; } = new List<string>();
    }
}