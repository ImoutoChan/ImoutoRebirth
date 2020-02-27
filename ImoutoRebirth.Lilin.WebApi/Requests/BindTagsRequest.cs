using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using ImoutoRebirth.Lilin.Core.Models;
using ImoutoRebirth.Lilin.Core.Models.FileInfoAggregate;
using ImoutoRebirth.Lilin.Services.CQRS.Commands;

namespace ImoutoRebirth.Lilin.WebApi.Requests
{
    public class BindTagsRequest
    {
        [Required]
        public IReadOnlyCollection<FileTagRequest> FileTags { get; set; } = default!;

        public SameTagHandleStrategy SameTagHandleStrategy { get; set; }
    }
}