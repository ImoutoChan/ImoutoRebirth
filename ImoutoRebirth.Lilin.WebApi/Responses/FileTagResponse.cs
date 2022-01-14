using ImoutoRebirth.Lilin.Core.Models;

namespace ImoutoRebirth.Lilin.WebApi.Responses
{
    public class FileTagResponse
    {
        public Guid FileId { get; }

        public TagResponse Tag { get; }

        public string Value { get; }

        public MetadataSource Source { get; }

        public FileTagResponse(Guid fileId, TagResponse tag, string value, MetadataSource source)
        {

            FileId = fileId;
            Tag = tag;
            Value = value;
            Source = source;
        }
    }
}