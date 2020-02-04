using System.Collections.Generic;

namespace ImoutoRebirth.Common.Domain
{
    public interface IEventStorage
    {
        void Add(IDomainEvent domainEvent);

        void AddRange(IEnumerable<IDomainEvent> domainEvents);

        IReadOnlyCollection<IDomainEvent> GetAll();
    }
}