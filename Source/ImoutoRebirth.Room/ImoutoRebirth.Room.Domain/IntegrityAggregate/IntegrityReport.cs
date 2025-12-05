using ImoutoRebirth.Room.Domain.CollectionAggregate;
using NodaTime;

namespace ImoutoRebirth.Room.Domain.IntegrityAggregate;

public class IntegrityReport
{
    private IntegrityReport(
        Guid reportId,
        Instant startedOn,
        ReportStatus status,
        int expectedTotalFileCount,
        int processedFileCount,
        string exportToFolder,
        IReadOnlyCollection<IntegrityReportCollection> collections)
    {
        ReportId = reportId;
        StartedOn = startedOn;
        Collections = collections;
        Status = status;
        ExportToFolder = exportToFolder;
        ExpectedTotalFileCount = expectedTotalFileCount;
        ProcessedFileCount = processedFileCount;
    }

    public Guid ReportId { get; }

    public Instant StartedOn { get; }

    public ReportStatus Status { get; private set; }

    public int ExpectedTotalFileCount { get; private set; }

    public int ProcessedFileCount { get; private set; }

    public IReadOnlyCollection<IntegrityReportCollection> Collections { get; }

    public string ExportToFolder { get; }

    public static IntegrityReport Create(
        Instant now,
        IReadOnlyCollection<Collection> collections,
        string? exportToFolder)
    {
        exportToFolder ??= GetDefaultExportToFolder();

        return new IntegrityReport(
            Guid.CreateVersion7(),
            now,
            ReportStatus.Created,
            0,
            0,
            exportToFolder,
            collections.Select(IntegrityReportCollection.Create).ToList());
    }

    public void SaveFileStatus(Guid collectionId, IntegrityReportFileStatus status)
    {
        var collection = Collections.Single(c => c.CollectionId == collectionId);
        collection.SaveStatus(status);

        ProcessedFileCount = Collections.Sum(c => c.ProcessedFileCount);
    }

    public void UpdateExpectedTotalFileCount(Guid collectionId, int expectedTotalCount)
    {
        var collection = Collections.Single(c => c.CollectionId == collectionId);
        collection.UpdateExpectedTotalFileCount(expectedTotalCount);

        ExpectedTotalFileCount = Collections.Sum(c => c.ExpectedTotalFileCount);
    }

    public void SetCollectionCompleted(Guid collectionId, IReadOnlyCollection<string> savedToPaths)
    {
        var collection = Collections.Single(c => c.CollectionId == collectionId);
        collection.SetCompleted(savedToPaths);
    }

    public void SetStatus(ReportStatus newStatus) => Status = newStatus;

    public static IntegrityReport Restore(
        Guid reportId,
        Instant startedOn,
        ReportStatus status,
        int expectedTotalFileCount,
        int processedFileCount,
        string exportToFolder,
        IReadOnlyCollection<IntegrityReportCollection> collections)
        => new(
            reportId,
            startedOn,
            status,
            expectedTotalFileCount,
            processedFileCount,
            exportToFolder,
            collections);

    private static string GetDefaultExportToFolder()
        => Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "ImoutoRebirth",
            "IntegrityReports");
}

public enum ReportStatus
{
    Created,
    Building,
    Paused,
    Completed,
    Cancelled
}
