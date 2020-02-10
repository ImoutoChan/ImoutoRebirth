using System.Collections.Generic;

namespace ImoutoRebirth.Lilin.WebApi.Requests
{
    public class CreateFileTagsRequest
    {
        public IReadOnlyCollection<FileTagRequest> FileTags { get; set; } = default!;
    }
}