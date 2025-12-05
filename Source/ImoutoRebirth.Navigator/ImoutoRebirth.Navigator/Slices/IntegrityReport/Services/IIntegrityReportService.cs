using ImoutoRebirth.Room.WebApi.Client;

namespace ImoutoRebirth.Navigator.Slices.IntegrityReport.Services;

public interface IIntegrityReportService
{
    Task<IReadOnlyCollection<IntegrityReportResult>> GetReportsAsync(int count, int skip);

    Task<Guid> CreateReportAsync(IReadOnlyCollection<Guid>? collectionIds, string? saveToFolder);

    Task PauseReportAsync(Guid reportId);

    Task ResumeReportAsync(Guid reportId);
}

