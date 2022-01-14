using ImoutoRebirth.Lilin.Core.Models;

namespace ImoutoRebirth.Lilin.WebApi.Requests
{
    public class FileTagRequest
    {
        public Guid TagId { get; set; }

        public Guid FileId { get; set; }

        public MetadataSource Source { get; set; }

        public string? Value { get; set; }
    }
}