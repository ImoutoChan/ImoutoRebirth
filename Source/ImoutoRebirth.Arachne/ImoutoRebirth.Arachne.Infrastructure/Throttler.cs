using System.Collections.Concurrent;

namespace ImoutoRebirth.Arachne.Infrastructure;

public class Throttler
{
    private static readonly ConcurrentDictionary<string, Throttler> Throttlers = new();

    public static Throttler Get(string key) => Throttlers.GetOrAdd(key, _ => new Throttler());

    private readonly SemaphoreSlim _locker = new(1);
    private DateTimeOffset _lastAccess = DateTimeOffset.MinValue;

    public async ValueTask UseAsync(TimeSpan delay)
    {
        await _locker.WaitAsync();
        try
        {
            var timePassedSinceLastCall = DateTimeOffset.UtcNow - _lastAccess;

            if (timePassedSinceLastCall < delay)
            {
                var wait = delay - timePassedSinceLastCall;;
                await Task.Delay(wait);
            }

            _lastAccess = DateTimeOffset.UtcNow;
        }
        finally
        {
            _locker.Release();
        }
    }
}
