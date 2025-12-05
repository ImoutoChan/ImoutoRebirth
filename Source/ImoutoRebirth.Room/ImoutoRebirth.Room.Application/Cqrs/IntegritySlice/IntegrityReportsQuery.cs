using ImoutoRebirth.Common.Cqrs.Abstract;
using ImoutoRebirth.Room.Domain.IntegrityAggregate;
using NodaTime;

namespace ImoutoRebirth.Room.Application.Cqrs.IntegritySlice;

public record IntegrityReportsQuery(int Count, int Skip) : IQuery<IReadOnlyCollection<IntegrityReportResult>>;

public record IntegrityReportResult(
    Guid ReportId,
    Instant StartedOn,
    ReportStatus Status,
    int ExpectedTotalFileCount,
    int ProcessedFileCount,
    string ExportToFolder,
    IReadOnlyCollection<IntegrityReportCollectionResult> Collections);

public record IntegrityReportCollectionResult(
    Guid CollectionId,
    string CollectionName,
    bool IsCompleted,
    int ExpectedTotalFileCount,
    int ProcessedFileCount,
    IReadOnlyCollection<string> ReportFiles);
