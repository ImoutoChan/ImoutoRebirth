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

        public static async Task<(TTaskResult Result, TWith With)> With<TTaskResult, TWith>(
            this Task<TTaskResult> task,
            TWith with)
        {
            var taskResult = await task;
            return (taskResult, with);
        }

        public static async Task<(TTaskResult Result, TWith With, TWith1 With1)> With<TTaskResult, TWith, TWith1>(
            this Task<TTaskResult> task,
            TWith with,
            TWith1 with1)
        {
            var taskResult = await task;
            return (taskResult, with, with1);
        }

        public static async Task<(TTaskResult Result, TWith With, TWith1 With1, TWith2 With2)>
            With<TTaskResult, TWith, TWith1, TWith2>(
                this Task<TTaskResult> task,
                TWith with,
                TWith1 with1,
                TWith2 with2)
        {
            var taskResult = await task;
            return (taskResult, with, with1, with2);
        }

        public static async Task<(TTaskResult Result, TWith With, TWith1 With1, TWith2 With2, TWith3 With3)>
            With<TTaskResult, TWith, TWith1, TWith2, TWith3>(
                this Task<TTaskResult> task,
                TWith with,
                TWith1 with1,
                TWith2 with2,
                TWith3 with3)
        {
            var taskResult = await task;
            return (taskResult, with, with1, with2, with3);
        }

        public static async
            Task<(TTaskResult Result, TWith With, TWith1 With1, TWith2 With2, TWith3 With3, TWith4 With4)>
            With<TTaskResult, TWith, TWith1, TWith2, TWith3, TWith4>(
                this Task<TTaskResult> task,
                TWith with,
                TWith1 with1,
                TWith2 with2,
                TWith3 with3,
                TWith4 with4)
        {
            var taskResult = await task;
            return (taskResult, with, with1, with2, with3, with4);
        }
    }
}