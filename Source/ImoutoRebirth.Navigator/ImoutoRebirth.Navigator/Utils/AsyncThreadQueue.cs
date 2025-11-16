using System.Threading.Channels;
using Serilog;

namespace ImoutoRebirth.Navigator.Utils;

public class AsyncThreadQueue : IDisposable
{
    private readonly Channel<Func<Task>> _channel;
    private readonly CancellationTokenSource _cts = new();
    private readonly Task _task1;
    private readonly Task _task2;

    public AsyncThreadQueue()
    {
        _channel = Channel.CreateUnbounded<Func<Task>>();
        _task1 = Task.Run(ThreadMethod);
        _task2 = Task.Run(ThreadMethod);
    }


    public void ClearQueue()
    {
        while (_channel.Reader.TryRead(out _)) { }
    }

    public void Add(Func<Task> action)
    {
        _channel.Writer.TryWrite(action);
    }

    private async Task ThreadMethod()
    {
        while (!_cts.Token.IsCancellationRequested)
        {
            try
            {
                await _channel.Reader.WaitToReadAsync(_cts.Token);

                if (!_channel.Reader.TryRead(out var action))
                    continue;

                try
                {
                    await action();
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "Error in method");
                }
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (ChannelClosedException)
            {
                break;
            }
        }
    }

    public void Dispose()
    {
        _channel.Writer.Complete();
        _cts.Dispose();
    }
}
