using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ImoutoRebirth.Lilin.WebApi.Requests
{
    public class BindTagsRequest
    {
        [Required]
        public IReadOnlyCollection<FileTagRequest> FileTags { get; set; } = default!;
    }
}