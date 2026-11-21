using System.Threading.Channels;
using Serilog;

namespace ImoutoRebirth.Navigator.Utils;

public class AsyncThreadQueue : IDisposable
{
    private readonly Channel<(Func<Task> Run, Action? Discard)> _channel;

    public AsyncThreadQueue()
    {
        _channel = Channel.CreateUnbounded<(Func<Task>, Action?)>();
        _ = Task.Run(ThreadMethod);
        _ = Task.Run(ThreadMethod);
    }

    public void ClearQueue()
    {
        while (_channel.Reader.TryRead(out var work))
            work.Discard?.Invoke();
    }

    public void Add(Func<Task> action)
    {
        _channel.Writer.TryWrite((action, null));
    }

    public Task<T> Run<T>(Func<T> action)
    {
        var result = new TaskCompletionSource<T>(TaskCreationOptions.RunContinuationsAsynchronously);

        Task Run()
        {
            try
            {
                result.TrySetResult(action());
            }
            catch (Exception e)
            {
                result.TrySetException(e);
            }

            return Task.CompletedTask;
        }

        if (!_channel.Writer.TryWrite((Run, () => result.TrySetCanceled())))
            result.TrySetCanceled();

        return result.Task;
    }

    private async Task ThreadMethod()
    {
        await foreach (var work in _channel.Reader.ReadAllAsync())
        {
            try
            {
                await work.Run();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error loading preview");
            }
        }
    }

    public void Dispose()
    {
        _channel.Writer.TryComplete();
        ClearQueue();
    }
}
