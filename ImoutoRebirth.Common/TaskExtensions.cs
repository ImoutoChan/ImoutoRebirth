using System.Collections.Generic;
using System.Threading.Tasks;

namespace ImoutoRebirth.Common
{
    public static class TaskExtensions
    {
        public static Task WhenAll(this IEnumerable<Task> tasks)
            => Task.WhenAll(tasks);

        public static Task<T[]> WhenAll<T>(this IEnumerable<Task<T>> tasks)
            => Task.WhenAll(tasks);
    }
}