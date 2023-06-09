namespace ImoutoRebirth.Common;

public static class EnumerableExtensions
{
    public static IEnumerable<T> DistinctBy<T, TKey>(this IEnumerable<T> source, Func<T, TKey> selector)
    {
        var hashSet = new HashSet<TKey>();

        foreach (var item in source)
        {
            var propertyValue = selector(item);
            if (hashSet.Add(propertyValue))
                yield return item;
        }
    }

    public static string JoinStrings<T>(this IEnumerable<T> source, Func<T, string> stringSelector, string separator) 
        => string.Join(separator, source.Select(stringSelector));
    
    public static T[] AsArray<T>(this T item) => new [] { item };
}
