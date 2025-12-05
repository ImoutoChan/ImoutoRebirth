using ImoutoRebirth.Room.Domain.IntegrityAggregate;
using NodaTime;

namespace ImoutoRebirth.Room.Database.Entities;

public class IntegrityReportEntity
{
    public Guid ReportId { get; init; }

    public Instant StartedOn { get; init; }

    public ReportStatus Status { get; set; }

    public int ExpectedTotalFileCount { get; set; }

    public int ProcessedFileCount { get; set; }

    public required string ExportToFolder { get; init; }

    public IList<IntegrityReportCollectionEntity> Collections { get; set; } = [];
}
