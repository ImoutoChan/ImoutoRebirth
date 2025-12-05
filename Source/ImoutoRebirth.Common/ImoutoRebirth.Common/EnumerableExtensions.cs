using System.Diagnostics.CodeAnalysis;

namespace ImoutoRebirth.Common;

public static class EnumerableExtensions
{
    public static T[] AsArray<T>(this T item) => [item];

    public static bool SafeAny<T>([NotNullWhen(true)] this IEnumerable<T>? source) => source != null && source.Any();

    public static bool SafeNone<T>([NotNullWhen(false)] this IEnumerable<T>? source) => source == null || !source.Any();

    public static IEnumerable<T> WhereNotNull<T>(this IEnumerable<T?> source) => source.Where(x => x != null)!;

    extension<T>(IEnumerable<T> source)
    {
        public bool None() => !source.Any();

        public bool None(Func<T, bool> predicate) => !source.Any(predicate);

        public string JoinStrings(Func<T, string> stringSelector, string separator)
            => string.Join(separator, source.Select(stringSelector));
    }

    extension(IEnumerable<string> source)
    {
        public bool ContainsIgnoreCase(string value)
            => source.Any(x => x.EqualsIgnoreCase(value));

        public bool ContainsAnyOfIgnoreCase(IReadOnlyCollection<string> value)
            => source.Any(x => value.ContainsIgnoreCase(x));

        public string JoinStrings(string separator = "") => string.Join(separator, source);
    }
}
