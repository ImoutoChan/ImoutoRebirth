namespace ImoutoRebirth.Common.Jobs.Progress;

public class ConsoleProgressReporter : ProgressReporter
{
    public ConsoleProgressReporter(int totalItemsToProcess) : base(totalItemsToProcess) { }

    protected override void ReportProgress(int processedCount, int totalItemsToProcess, TimeSpan elapsed, TimeSpan eta)
        => Console.WriteLine(
            $"Processed {processedCount:000000} / {totalItemsToProcess:000000} "
            + $"({processedCount * 1.0 / totalItemsToProcess:00.00%}) "
            + $"ETA {eta:hh\\:mm\\:ss}");

    protected override void ReportCompleted(int totalItemsToProcess, TimeSpan elapsed)
        => Console.WriteLine($@"Completed in {elapsed:hh\:mm\:ss}");
}
