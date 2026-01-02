namespace ImoutoRebirth.Common.Domain;

public static class AggregateExtensions
{
    public static async Task<T> GetAggregateOrThrow<T>(this Task<T?> aggregateGetter)
    {
        var aggregate = await aggregateGetter;

        if (aggregate == null)
        {
            throw AggregateNotFoundException.Create<T>();
        }

        return aggregate;
    }

    public static async Task<T> GetAggregateOrThrow<T, TId>(
        this Task<T?> aggregateGetter,
        TId id)
    {
        var aggregate = await aggregateGetter;

        if (aggregate == null)
        {
            throw AggregateNotFoundException.Create<T>(id);
        }

        return aggregate;
    }

    public static async ValueTask<T> GetAggregateOrThrow<T, TId>(
        this ValueTask<T?> aggregateGetter,
        TId id)
    {
        var aggregate = await aggregateGetter;

        if (aggregate == null)
        {
            throw AggregateNotFoundException.Create<T>(id);
        }

        return aggregate;
    }
}

public class AggregateNotFoundException : DomainException
{
    public string AggregateName { get; }

    public string? AggregateId { get; }

    private AggregateNotFoundException(string aggregateName, string? aggregateId)
        : base(
            $"The aggregate {aggregateName}" +
            $"{(aggregateId == null ? "" : " with Id " + aggregateId)} was not found")
    {
        AggregateName = aggregateName;
        AggregateId = aggregateId;
    }

    public static AggregateNotFoundException Create<T>(object? id = null)
        => new(typeof(T).Name, id?.ToString());
}
