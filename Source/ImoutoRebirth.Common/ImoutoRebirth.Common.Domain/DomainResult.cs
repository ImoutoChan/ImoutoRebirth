using System.Collections;

namespace ImoutoRebirth.Common.Domain;

public class DomainResult : IEnumerable<IDomainEvent>
{
    protected readonly List<IDomainEvent> Events;

    public IReadOnlyCollection<IDomainEvent> EventsCollection => Events;

    public DomainResult() => Events = new List<IDomainEvent>();
    
    public DomainResult(params IDomainEvent[] events) => Events = new List<IDomainEvent>(events);

    public static DomainResult Empty => new();
    
    public void Add(IDomainEvent domainEvent) => Events.Add(domainEvent);
    
    IEnumerator<IDomainEvent> IEnumerable<IDomainEvent>.GetEnumerator() => Events.GetEnumerator();
    
    public IEnumerator GetEnumerator() => Events.GetEnumerator();
}

public class DomainResult<T> : DomainResult
{
    public T Result { get; }

    public DomainResult(DomainResult domainResult, T result)
    {
        Result = result;
        Events.AddRange(domainResult.EventsCollection);
    }

    public DomainResult(T result)
    {
        Result = result;
    }
}
