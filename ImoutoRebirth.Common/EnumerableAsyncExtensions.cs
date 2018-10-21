using System.Collections.Generic;
using System.Threading.Tasks;

namespace ImoutoRebirth.Common
{
    public static class EnumerableAsyncExtensions
    {
        public static async Task<T[]> ToArrayAsync<T>(
            this IEnumerable<Task<T>> source, 
            bool sequential = false)
        {
            if (!sequential)
                return await Task.WhenAll(source);

            var results = new List<T>();
            foreach (var task in source)
            {
                results.Add(await task);
            }

            return results.ToArray();
        }
    }
}