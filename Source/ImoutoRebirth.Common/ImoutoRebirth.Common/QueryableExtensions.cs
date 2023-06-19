using System.Linq.Expressions;

namespace ImoutoRebirth.Common;

public static class QueryableExtensions
{
    public static IQueryable<T> WhereIf<T>(
        this IQueryable<T> source,
        bool condition,
        Expression<Func<T, bool>> predicate)
    {
        if (condition)
        {
            return source.Where(predicate);
        }

        return source;
    }

    public static IQueryable<T> WhereIfNotNull<T, TP>(
        this IQueryable<T> source,
        TP? item,
        Expression<Func<T, bool>> predicate)
    {
        if (item != null)
        {
            return source.Where(predicate);
        }

        return source;
    }

    public static IQueryable<T> WhereIfNotNullOrWhiteSpace<T>(
        this IQueryable<T> source,
        string? item,
        Expression<Func<T, bool>> predicate)
    {
        if (!string.IsNullOrWhiteSpace(item))
        {
            return source.Where(predicate);
        }

        return source;
    }

    public static IQueryable<T> WhereIfNotNullOrEmpty<T, TP>(
        this IQueryable<T> source,
        IReadOnlyCollection<TP>? parameters,
        Expression<Func<T, bool>> predicate)
    {
        if (parameters != null && parameters.Any())
        {
            return source.Where(predicate);
        }

        return source;
    }

    public static IQueryable<T> WhereIfNullOrWhiteSpace<T>(
        this IQueryable<T> source,
        string? item,
        Expression<Func<T, bool>> predicate)
    {
        if (string.IsNullOrWhiteSpace(item))
        {
            return source.Where(predicate);
        }

        return source;
    }
}
