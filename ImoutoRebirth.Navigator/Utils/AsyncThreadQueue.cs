using System.Collections.Concurrent;
using System.Diagnostics;

namespace ImoutoRebirth.Navigator.Utils;

public class AsyncThreadQueue : IDisposable
{
    private readonly Task _task;
    private ConcurrentQueue<Func<Task>> _queue;
    private bool _isRunning = true;

    public AsyncThreadQueue()
    {
        _queue = new ConcurrentQueue<Func<Task>>();
        _task = Task.Run(ThreadMethod);
    }


    public void ClearQueue()
    {
        _queue = new ConcurrentQueue<Func<Task>>();
    }

    public void Add(Func<Task> action)
    {
        _queue.Enqueue(action);
    }

    private Func<Task>? GetFromQueue()
    {
        _queue.TryDequeue(out var result);

        return result;
    }

    private async Task ThreadMethod()
    {
        while (_isRunning)
        {
            var action = GetFromQueue();

            if (action != null)
            {
                try
                {
                    await action.Invoke();
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("Error in method: {0}", ex.Message);
                }

            }
            else
            {
                SpinWait.SpinUntil(() => !_queue.IsEmpty);
            }
        }
    }

    public void Dispose()
    {
        _isRunning = false;
    }
}