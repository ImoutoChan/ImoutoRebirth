using ImoutoRebirth.Common.Domain;

namespace ImoutoRebirth.Meido.Core.ParsingStatus.Events
{
    public class ParsingStatusCreated : IDomainEvent
    {
        public ParsingStatus Created { get; }

        public ParsingStatusCreated(ParsingStatus created)
        {
            Created = created;
        }
    }
}