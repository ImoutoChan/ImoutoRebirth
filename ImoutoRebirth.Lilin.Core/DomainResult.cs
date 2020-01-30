using System.Collections.Generic;
using ImoutoRebirth.Common.Domain;

namespace ImoutoRebirth.Lilin.Core
{
    public class DomainResult
    {
        protected readonly List<IDomainEvent> Events;

        public IReadOnlyCollection<IDomainEvent> EventsCollection => Events;

        public DomainResult()
        {
            Events = new List<IDomainEvent>();
        }

        public void Add(IDomainEvent domainEvent) => Events.Add(domainEvent);
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
}