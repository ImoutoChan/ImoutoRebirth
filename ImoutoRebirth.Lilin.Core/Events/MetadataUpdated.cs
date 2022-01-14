using ImoutoRebirth.Common;
using ImoutoRebirth.Common.Domain;
using ImoutoRebirth.Lilin.Core.Models;

namespace ImoutoRebirth.Lilin.Core.Events
{
    public class MetadataUpdated : IDomainEvent
    {
        public MetadataUpdated(Guid fileId, MetadataSource metadataSource)
        {
            ArgumentValidator.IsEnumDefined(() => metadataSource);
            ArgumentValidator.Requires(() => fileId != default, nameof(fileId));

            FileId = fileId;
            MetadataSource = metadataSource;
        }

        public Guid FileId { get; }

        public MetadataSource MetadataSource { get; }
    }
}