using System.Runtime.CompilerServices;
using System.Windows.Threading;

namespace ImoutoRebirth.Common.WPF;

public static class T
{
    private static readonly Dictionary<string, DispatcherTimer> Timers = new();

    public static void Debounce(
        int milliseconds,
        Action action,
        [CallerMemberName] string caller = "")
    {
        var targetId = action.Target is null ? 0 : RuntimeHelpers.GetHashCode(action.Target);
        var key = $"{caller}#{targetId}";

        if (!Timers.TryGetValue(key, out var timer))
        {
            timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(milliseconds)
            };

            timer.Tick += (_, _) =>
            {
                timer.Stop();
                action();
            };

            Timers[key] = timer;
        }
        else
        {
            timer.Interval = TimeSpan.FromMilliseconds(milliseconds);
        }

        timer.Stop();
        timer.Start();
    }
}
