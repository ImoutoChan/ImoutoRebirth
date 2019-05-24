using System.Collections.Generic;

namespace ImoutoRebirth.Common.Domain
{
    public interface IEventStorage
    {
        void Add(IDomainEvent domainEvent);

        IReadOnlyCollection<IDomainEvent> GetAll();
    }
}