using System.Collections.Concurrent;
using ImoutoRebirth.Room.Domain.CollectionAggregate;

namespace ImoutoRebirth.Room.Domain.IntegrityAggregate;

public class IntegrityReportCollection
{
    private readonly ConcurrentBag<IntegrityReportFileStatus> _calculatedFileStatuses;

    private IntegrityReportCollection(
        Guid collectionId,
        string collectionName,
        ConcurrentBag<IntegrityReportFileStatus> calculatedFileStatuses,
        bool isCompleted,
        int expectedTotalFileCount,
        int processedFileCount,
        IReadOnlyCollection<string> reportExportedFiles)
    {
        CollectionId = collectionId;
        CollectionName = collectionName;
        IsCompleted = isCompleted;
        ExpectedTotalFileCount = expectedTotalFileCount;
        ProcessedFileCount = processedFileCount;
        ReportExportedFiles = reportExportedFiles;

        _calculatedFileStatuses = calculatedFileStatuses;
    }

    public Guid CollectionId { get; }

    public string CollectionName { get; }

    public bool IsCompleted { get; private set; }

    public int ExpectedTotalFileCount { get; private set; }

    public int ProcessedFileCount { get; private set; }

    public IReadOnlyCollection<string> ReportExportedFiles { get; private set; }

    public IReadOnlyCollection<IntegrityReportFileStatus> CalculatedFileStatuses => _calculatedFileStatuses;

    public static IntegrityReportCollection Create(Collection collection)
    {
        return new IntegrityReportCollection(
            collection.Id,
            collection.Name,
            [],
            false,
            0,
            0,
            []);
    }

    internal void SaveStatus(IntegrityReportFileStatus status)
    {
        _calculatedFileStatuses.Add(status);
        ProcessedFileCount = _calculatedFileStatuses.Count;
    }

    internal void UpdateExpectedTotalFileCount(int expectedTotalCount) => ExpectedTotalFileCount = expectedTotalCount;

    internal void SetCompleted(IReadOnlyCollection<string> savedToPaths)
    {
        IsCompleted = true;
        ReportExportedFiles = savedToPaths;
    }

    public IReadOnlyCollection<Guid> GetAlreadyProcessedFileIds()
        => CalculatedFileStatuses
            .Select(x => x.FileId)
            .ToHashSet();

    public static IntegrityReportCollection Restore(
        Guid collectionId,
        string collectionName,
        IEnumerable<IntegrityReportFileStatus> calculatedFileStatuses,
        bool isCompleted,
        int expectedTotalFileCount,
        int processedFileCount,
        IReadOnlyCollection<string> reportExportedFiles)
        => new(
            collectionId,
            collectionName,
            new ConcurrentBag<IntegrityReportFileStatus>(calculatedFileStatuses),
            isCompleted,
            expectedTotalFileCount,
            processedFileCount,
            reportExportedFiles);
}
