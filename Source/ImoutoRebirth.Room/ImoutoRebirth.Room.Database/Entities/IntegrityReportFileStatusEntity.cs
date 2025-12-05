using ImoutoRebirth.Room.Domain.IntegrityAggregate;

namespace ImoutoRebirth.Room.Database.Entities;

public class IntegrityReportFileStatusEntity
{
    public Guid ReportId { get; init; }

    public Guid CollectionId { get; init; }

    public Guid FileId { get; init; }

    public required string ExpectedFullPath { get; init; }

    public required string FileName { get; init; }

    public bool IsPresent { get; init; }

    public required string ExpectedHash { get; init; }

    public string? ActualHash { get; init; }

    public string? ReadingProblem { get; init; }

    public IntegrityStatus Status { get; init; }

    public IntegrityReportCollectionEntity? Collection { get; init; }
}
