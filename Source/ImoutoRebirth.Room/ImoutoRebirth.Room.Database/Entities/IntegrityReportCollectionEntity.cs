namespace ImoutoRebirth.Room.Database.Entities;

public class IntegrityReportCollectionEntity
{
    public Guid ReportId { get; init; }

    public Guid CollectionId { get; init; }

    public required string CollectionName { get; init; }

    public int ProcessedFileCount { get; set; }

    public int ExpectedTotalFileCount { get; set; }

    public bool IsCompleted { get; set; }

    public required IReadOnlyCollection<string> ReportExportedFiles { get; set; }

    public IntegrityReportEntity? Report { get; init; }

    public IList<IntegrityReportFileStatusEntity> FileStatuses { get; init; } = [];
}
