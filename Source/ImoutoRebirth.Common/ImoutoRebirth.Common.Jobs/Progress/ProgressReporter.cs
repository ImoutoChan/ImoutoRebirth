using System.Diagnostics;

namespace ImoutoRebirth.Common.Jobs.Progress;

public abstract class ProgressReporter : IDisposable
{
    private readonly Stopwatch _stopwatch = new();
    private readonly int _totalItemsToProcess;
    private int _processedCount;

    protected ProgressReporter(int totalItemsToProcess, bool autoStart = true)
    {
        _totalItemsToProcess = totalItemsToProcess;

        if (autoStart)
        {
            _stopwatch.Start();
        }
    }

    public void ManualStart() => _stopwatch.Start();

    public void ReportItemProcessed()
    {
        Interlocked.Increment(ref _processedCount);

        var etaMilliseconds = _stopwatch.ElapsedMilliseconds * _totalItemsToProcess / _processedCount
                              - _stopwatch.ElapsedMilliseconds;

        ReportProgress(
            _processedCount,
            _totalItemsToProcess,
            _stopwatch.Elapsed,
            TimeSpan.FromMilliseconds(etaMilliseconds));
    }

    public void Dispose()
    {
        _stopwatch.Stop();
        ReportCompleted(_totalItemsToProcess, _stopwatch.Elapsed);
    }

    protected abstract void ReportProgress(int processedCount, int totalItemsToProcess, TimeSpan elapsed, TimeSpan eta);

    protected abstract void ReportCompleted(int totalItemsToProcess, TimeSpan elapsed);
}