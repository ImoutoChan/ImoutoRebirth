using System;
using System.Collections.Generic;

namespace ImoutoRebirth.Common
{
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
    }
}