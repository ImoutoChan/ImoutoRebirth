using ImoutoRebirth.Common.Cqrs.Abstract;
using ImoutoRebirth.Common.Jobs;
using ImoutoRebirth.Room.Application.Services;
using ImoutoRebirth.Room.Domain.IntegrityAggregate;

namespace ImoutoRebirth.Room.Application.Cqrs.IntegritySlice;

public record PauseIntegrityReportJobCommand(Guid ReportId) : ICommand;

internal class PauseIntegrityReportJobCommandHandler : ICommandHandler<PauseIntegrityReportJobCommand>
{
    private readonly IIntegrityReportRepository _integrityReportRepository;
    private readonly IRunningJobs<RoomJobType> _runningJobs;

    public PauseIntegrityReportJobCommandHandler(
        IIntegrityReportRepository integrityReportRepository,
        IRunningJobs<RoomJobType> runningJobs)
    {
        _integrityReportRepository = integrityReportRepository;
        _runningJobs = runningJobs;
    }

    public async Task Handle(PauseIntegrityReportJobCommand command, CancellationToken ct)
    {
        var reportId = command.ReportId;
        var report = await _integrityReportRepository.Get(reportId);

        _runningJobs.Interrupt(RoomJobType.IntegrityReportBuilder);
        report.SetStatus(ReportStatus.Paused);
        await _integrityReportRepository.Update(report);
        _runningJobs.Interrupt(RoomJobType.IntegrityReportBuilder);
    }
}
