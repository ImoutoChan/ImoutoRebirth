using ImoutoRebirth.Common.Cqrs.Abstract;
using ImoutoRebirth.Room.Application.Services;
using ImoutoRebirth.Room.Domain.IntegrityAggregate;

namespace ImoutoRebirth.Room.Application.Cqrs.IntegritySlice;

public record ResumeIntegrityReportJobCommand(Guid ReportId) : ICommand;

internal class ResumeIntegrityReportJobCommandHandler : ICommandHandler<ResumeIntegrityReportJobCommand>
{
    private readonly IIntegrityReportRepository _integrityReportRepository;

    public ResumeIntegrityReportJobCommandHandler(
        IIntegrityReportRepository integrityReportRepository)
    {
        _integrityReportRepository = integrityReportRepository;
    }

    public async Task Handle(ResumeIntegrityReportJobCommand command, CancellationToken ct)
    {
        var reportId = command.ReportId;
        var report = await _integrityReportRepository.Get(reportId);

        report.SetStatus(ReportStatus.Created);
        await _integrityReportRepository.Update(report);
    }
}
