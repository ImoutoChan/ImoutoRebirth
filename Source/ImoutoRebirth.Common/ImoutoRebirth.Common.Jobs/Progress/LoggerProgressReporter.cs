using Microsoft.Extensions.Logging;

namespace ImoutoRebirth.Common.Jobs.Progress;

public class LoggerProgressReporter : ProgressReporter
{
    private readonly ILogger _logger;

    public LoggerProgressReporter(ILoggerFactory logger, int totalItemsToProcess)
        : base(totalItemsToProcess)
        => _logger = logger.CreateLogger<LoggerProgressReporter>();

    protected override void ReportProgress(int processedCount, int totalItemsToProcess, TimeSpan elapsed, TimeSpan eta)
        => _logger.LogDebug(
            "Processed {Processed} / {Total} ({Progress}) ETA {Eta}",
            processedCount,
            totalItemsToProcess,
            (processedCount * 100.0m / totalItemsToProcess).ToString("00.00") + "%",
            eta.ToString(@"hh\:mm\:ss"));

    protected override void ReportCompleted(int totalItemsToProcess, TimeSpan elapsed)
        => _logger.LogInformation(
            "Completed processing {Total} items in {Elapsed}",
            totalItemsToProcess,
            elapsed.ToString(@"hh\:mm\:ss"));
}
