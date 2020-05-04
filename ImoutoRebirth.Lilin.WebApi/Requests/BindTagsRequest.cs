using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using ImoutoRebirth.Lilin.Core.Models.FileInfoAggregate;

namespace ImoutoRebirth.Lilin.WebApi.Requests
{
    public class BindTagsRequest
    {
        [Required]
        public IReadOnlyCollection<FileTagRequest> FileTags { get; set; } = default!;

        public SameTagHandleStrategy SameTagHandleStrategy { get; set; }
    }
}