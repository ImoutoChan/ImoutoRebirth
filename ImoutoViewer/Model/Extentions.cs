using GihanSoft.String;

namespace ImoutoViewer.Model;

internal static class Extentions
{
    public static IOrderedEnumerable<TSource> OrderByWithDirection<TSource, TKey>(
        this IEnumerable<TSource> source,
        Func<TSource, TKey> keySelector,
        bool descending)
    {
        var enumerable = source.ToList();
        var first = enumerable.FirstOrDefault();
        
        if (first is string)
        {
            return descending
                ? enumerable.OrderByDescending(
                    x => (string)(object)keySelector(x)!,
                    NaturalComparer.InvariantCultureIgnoreCase)
                : enumerable.OrderBy(
                    x => (string)(object)keySelector(x)!, 
                    NaturalComparer.InvariantCultureIgnoreCase);
        }
        
        return descending
            ? enumerable.OrderByDescending(keySelector)
            : enumerable.OrderBy(keySelector);
    }
}
