namespace ImoutoRebirth.Common;

public static class ExtraTaskExtensions
{
    public static Task WhenAll(this IEnumerable<Task> tasks)
        => Task.WhenAll(tasks);

    public static Task<T[]> WhenAll<T>(this IEnumerable<Task<T>> tasks)
        => Task.WhenAll(tasks);

    public static async Task WhenAllSequential(this IEnumerable<Task> tasks)
    {
        foreach (var task in tasks)
        {
            await task;
        }
    }

    public static async Task<T[]> WhenAllSequential<T>(this IEnumerable<Task<T>> tasks)
    {
        var taskArray = tasks as Task<T>[] ?? tasks.ToArray();

        foreach (var task in taskArray)
        {
            await task;
        }

        return taskArray.Select(x => x.Result).ToArray();
    }

    public static async Task WhenAllWithDegreeOfParallelism(
        this IEnumerable<Task> tasks,
        int degreeOfParallelism,
        CancellationToken ct)
    {
        using var enumerator = tasks.GetEnumerator();
        var runningTasks = new List<Task>();

        while (enumerator.MoveNext())
        {
            ct.ThrowIfCancellationRequested();
            runningTasks.Add(enumerator.Current);
            if (runningTasks.Count < degreeOfParallelism)
                continue;

            var completed = await Task.WhenAny(runningTasks);
            runningTasks.Remove(completed);
        }

        await Task.WhenAll(runningTasks);
    }

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
