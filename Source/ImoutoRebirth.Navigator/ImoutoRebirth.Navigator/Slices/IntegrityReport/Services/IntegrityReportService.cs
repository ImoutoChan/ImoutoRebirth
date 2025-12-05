using ImoutoRebirth.Room.WebApi.Client;

namespace ImoutoRebirth.Navigator.Slices.IntegrityReport.Services;

internal class IntegrityReportService : IIntegrityReportService
{
    private readonly IntegrityReportsClient _client;

    public IntegrityReportService(IntegrityReportsClient client) => _client = client;

    public async Task<IReadOnlyCollection<IntegrityReportResult>> GetReportsAsync(int count, int skip)
        => await _client.GetIntegrityReportsAsync(count, skip);

    public async Task<Guid> CreateReportAsync(IReadOnlyCollection<Guid>? collectionIds, string? saveToFolder)
    {
        var command = new CreateIntegrityReportJobCommand(collectionIds, saveToFolder);
        return await _client.BuildIntegrityReportAsync(command);
    }

    public async Task PauseReportAsync(Guid reportId)
        => await _client.PauseIntegrityReportAsync(reportId);

    public async Task ResumeReportAsync(Guid reportId)
        => await _client.ResumeIntegrityReportAsync(reportId);
}

