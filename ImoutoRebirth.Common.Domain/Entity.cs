using System.Collections.Generic;

namespace ImoutoRebirth.Common.Domain;

public abstract class Entity
{
    private readonly List<IDomainEvent> _events = new List<IDomainEvent>();

    protected Entity()
    {
    }

    public IReadOnlyCollection<IDomainEvent> Events => _events;

    protected void Add(IDomainEvent domainEvent) => _events.Add(domainEvent);
}