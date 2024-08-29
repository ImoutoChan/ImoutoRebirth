using System.Diagnostics.CodeAnalysis;

namespace ImoutoRebirth.Common;

public static class EnumerableExtensions
{
    public static string JoinStrings<T>(this IEnumerable<T> source, Func<T, string> stringSelector, string separator) 
        => string.Join(separator, source.Select(stringSelector));
    
    public static T[] AsArray<T>(this T item) => [item];

    public static bool SafeAny<T>([NotNullWhen(true)] this IEnumerable<T>? source) => source != null && source.Any();

    public static bool None<T>(this IEnumerable<T> source) => !source.Any();

    public static bool None<T>(this IEnumerable<T> source, Func<T, bool> predicate) => !source.Any(predicate);
    
    public static bool SafeNone<T>([NotNullWhen(false)] this IEnumerable<T>? source) => source == null || !source.Any();

    public static IEnumerable<T> WhereNotNull<T>(this IEnumerable<T?> source) => source.Where(x => x != null)!;

    public static IEnumerable<T> Shuffle<T>(this IEnumerable<T> source)
    {
        var rng = Random.Shared;

        var elements = source.ToArray();
        for (var i = elements.Length - 1; i >= 0; i--)
        {
            var swapIndex = rng.Next(i + 1);
            yield return elements[swapIndex];
            elements[swapIndex] = elements[i];
        }
    }

}
