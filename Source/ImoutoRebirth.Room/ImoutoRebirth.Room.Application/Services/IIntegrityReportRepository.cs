using ImoutoRebirth.Room.Domain.IntegrityAggregate;

namespace ImoutoRebirth.Room.Application.Services;

public interface IIntegrityReportRepository
{
    Task<IntegrityReport?> GetFirstUnfinished();

    Task<IntegrityReport> Get(Guid reportId);

    Task Create(IntegrityReport report);

    Task Update(IntegrityReport report);

    Task ReportFiles(
        IReadOnlyCollection<IntegrityReportFileStatus> fileStatuses,
        Guid reportId,
        Guid collectionId);

    Task DeleteFiles(Guid reportId);
}
