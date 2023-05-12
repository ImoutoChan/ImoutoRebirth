using ImoutoRebirth.Common.Domain;

namespace ImoutoRebirth.Meido.Core.ParsingStatus.Events;

public class MetadataNotFound : IDomainEvent
{
    public ParsingStatus Created { get; }

    public MetadataNotFound(ParsingStatus created)
    {
        Created = created;
    }
}