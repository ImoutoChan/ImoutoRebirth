using System.Collections.Generic;

namespace ImoutoProject.Common.Cqrs.Behaviors
{
    public interface IEventStorage
    {
        void Add(IDomainEvent domainEvent);

        IReadOnlyCollection<IDomainEvent> GetAll();
    }
}