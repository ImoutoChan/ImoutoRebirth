namespace ImoutoRebirth.Navigator.Extensions;

public static class EnumerableCancellationExtensions
{
    public static IEnumerable<T> WithCancellation<T>(this IEnumerable<T> source, CancellationToken token)
    {
        if (source == null)
            throw new ArgumentNullException(nameof(source));

        foreach (var item in source)
        {
            token.ThrowIfCancellationRequested();
            yield return item;
        }
    }
    
    public static IEnumerable<T> SkipExceptions<T>(this IEnumerable<T> values)
    {
        using var enumerator = values.GetEnumerator();
        var next = true;
        while (next)
        {
            try
            {
                next = enumerator.MoveNext();
            }
            catch
            {
                continue;
            }

            if (next) yield return enumerator.Current;
        }
    }
}
