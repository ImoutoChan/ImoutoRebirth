using ImoutoRebirth.Common.Domain;

namespace ImoutoRebirth.Meido.Core.ParsingStatus.Events
{
    public class UpdateRequested : IDomainEvent
    {
        public ParsingStatus Entity { get; }

        public UpdateRequested(ParsingStatus entity)
        {
            Entity = entity;
        }
    }
}