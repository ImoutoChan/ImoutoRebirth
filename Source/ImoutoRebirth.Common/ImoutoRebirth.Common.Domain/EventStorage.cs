namespace ImoutoRebirth.Common.Domain;

public class EventStorage : IEventStorage
{
    private readonly List<IDomainEvent> _events = new();
    private Guid? _marked;

    public void Add(IDomainEvent domainEvent) => _events.Add(domainEvent);

    public void AddRange(IEnumerable<IDomainEvent> domainEvents)
        => _events.AddRange(domainEvents);

    public IReadOnlyCollection<IDomainEvent> GetAll(Guid mark)
    {
        if (mark != _marked)
        {
            return Array.Empty<IDomainEvent>();
        }

        var events = _events.ToList();
        _events.Clear();
        _marked = null;
        return events;
    }

    public void Mark(Guid mark) => _marked ??= mark;
}
