﻿namespace ImoutoRebirth.Common.Domain;

public class EventStorage : IEventStorage
{
    private readonly List<IDomainEvent> _events = new List<IDomainEvent>();

    public void Add(IDomainEvent domainEvent) => _events.Add(domainEvent);

    public void AddRange(IEnumerable<IDomainEvent> domainEvents)
    {
        foreach (var domainEvent in domainEvents)
        {
            _events.Add(domainEvent);
        }
    }

    public IReadOnlyCollection<IDomainEvent> GetAll() => _events;
}