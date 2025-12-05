using ImoutoRebirth.Room.Application.Services;
using ImoutoRebirth.Room.DataAccess.Mappers;
using ImoutoRebirth.Room.Database;
using Microsoft.EntityFrameworkCore;
using ImoutoRebirth.Room.Domain.IntegrityAggregate;

namespace ImoutoRebirth.Room.DataAccess.Repositories;

public class IntegrityReportRepository : IIntegrityReportRepository
{
    private readonly RoomDbContext _roomDbContext;

    public IntegrityReportRepository(RoomDbContext roomDbContext)
        => _roomDbContext = roomDbContext;

    public async Task<IntegrityReport?> GetFirstUnfinished()
    {
        var entity = await _roomDbContext.IntegrityReports
            .AsNoTracking()
            .Include(x => x.Collections)
                .ThenInclude(x => x.FileStatuses)
            .Where(x => x.Status == ReportStatus.Created || x.Status == ReportStatus.Building)
            .OrderBy(x => x.StartedOn)
            .FirstOrDefaultAsync();

        return entity?.ToModel();
    }

    public async Task<IntegrityReport> Get(Guid reportId)
    {
        var entity = await _roomDbContext.IntegrityReports
            .AsNoTracking()
            .Where(x => x.ReportId == reportId)
            .Include(x => x.Collections)
            .FirstAsync();

        return entity.ToModel();
    }

    public async Task Create(IntegrityReport report)
    {
        _roomDbContext.ChangeTracker.Clear();
        var entity = report.ToEntity();
        await _roomDbContext.IntegrityReports.AddAsync(entity);
        await _roomDbContext.SaveChangesAsync();
    }

    public async Task Update(IntegrityReport report)
    {
        _roomDbContext.ChangeTracker.Clear();
        var entity = await _roomDbContext.IntegrityReports
            .Include(x => x.Collections)
                .ThenInclude(x => x.FileStatuses)
            .SingleAsync(x => x.ReportId == report.ReportId);

        entity.UpdateCollections(report);

        await _roomDbContext.SaveChangesAsync();
    }

    public async Task ReportFiles(
        IReadOnlyCollection<IntegrityReportFileStatus> fileStatuses,
        Guid reportId,
        Guid collectionId)
    {
        _roomDbContext.ChangeTracker.Clear();
        var files = fileStatuses.Select(x => x.ToEntity(reportId, collectionId)).ToList();
        _roomDbContext.IntegrityReportFileStatuses.AddRange(files);
        await _roomDbContext.SaveChangesAsync();
    }

    public async Task DeleteFiles(Guid reportId)
    {
        await _roomDbContext
            .IntegrityReportFileStatuses
            .Where(x => x.ReportId == reportId)
            .ExecuteDeleteAsync();
    }
}
