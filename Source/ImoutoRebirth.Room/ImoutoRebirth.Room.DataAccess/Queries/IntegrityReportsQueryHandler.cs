using ImoutoRebirth.Common.Cqrs.Abstract;
using ImoutoRebirth.Room.Application.Cqrs.IntegritySlice;
using ImoutoRebirth.Room.Database;
using Microsoft.EntityFrameworkCore;

namespace ImoutoRebirth.Room.DataAccess.Queries;

internal class IntegrityReportsQueryHandler
    : IQueryHandler<IntegrityReportsQuery, IReadOnlyCollection<IntegrityReportResult>>
{
    private readonly RoomDbContext _roomDbContext;

    public IntegrityReportsQueryHandler(RoomDbContext roomDbContext)
        => _roomDbContext = roomDbContext;

    public async Task<IReadOnlyCollection<IntegrityReportResult>> Handle(
        IntegrityReportsQuery query,
        CancellationToken ct)
    {
        var reports = await _roomDbContext.IntegrityReports
            .AsNoTracking()
            .Include(x => x.Collections)
            .OrderByDescending(x => x.StartedOn)
            .Skip(query.Skip)
            .Take(query.Count)
            .ToListAsync(cancellationToken: ct);

        return reports
            .Select(x => new IntegrityReportResult(
                x.ReportId,
                x.StartedOn,
                x.Status,
                x.ExpectedTotalFileCount,
                x.ProcessedFileCount,
                x.ExportToFolder,
                x.Collections
                    .Select(y => new IntegrityReportCollectionResult(
                        y.CollectionId,
                        y.CollectionName,
                        y.IsCompleted,
                        y.ExpectedTotalFileCount,
                        y.ProcessedFileCount,
                        y.ReportExportedFiles))
                    .ToList()))
            .ToList();
    }
}
