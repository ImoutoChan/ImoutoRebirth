namespace ImoutoRebirth.Navigator.Extensions;

internal static class EnumerableCancellationExtensions
{
    public static IEnumerable<T> WithCancellation<T>(this IEnumerable<T> source, CancellationToken token)
    {
        ArgumentNullException.ThrowIfNull(source);

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

            if (next) 
                yield return enumerator.Current;
        }
    }
}
