using AwesomeAssertions;
using ImoutoRebirth.Room.Application.Cqrs;
using ImoutoRebirth.Room.Application.Cqrs.IntegritySlice;
using ImoutoRebirth.Room.Domain.IntegrityAggregate;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace ImoutoRebirth.Room.IntegrationTests;

public partial class IntegrityReportTests
{
    [Fact]
    public async Task BuildReport_WhenPaused_ShouldSaveProgress()
    {
        // arrange
        var (collectionId, sourceFolderPath, _) = await CreateDefaultCollection();
        var exportFolder = Path.Combine(_webApp.TestsTempLocation, "integrity-reports", Guid.NewGuid().ToString());

        var file1 = new FileInfo(
            Path.Combine(_webApp.TestsLocation, "Resources", "file1-5f30f9953332c230d11e3f26db5ae9a0.jpg"));
        var file2 = new FileInfo(
            Path.Combine(_webApp.TestsLocation, "Resources", "file2-09e56a8fd9d1e8beb62c50e6945632bf.jpg"));

        file1.CopyTo(Path.Combine(sourceFolderPath, file1.Name));
        file2.CopyTo(Path.Combine(sourceFolderPath, file2.Name));
        await _mediator.Send(new OverseeCommand());

        var reportId = await _mediator.Send(new CreateIntegrityReportJobCommand([collectionId], exportFolder));

        // act
        var buildTask = _reportJobRunner.BuildNextUnfinishedReport(CancellationToken.None);

        await Task.Delay(50);
        await _mediator.Send(new PauseIntegrityReportJobCommand(reportId));
        await buildTask;

        // assert
        _context.ChangeTracker.Clear();
        var report = await _context.IntegrityReports
            .Include(x => x.Collections)
            .FirstOrDefaultAsync(x => x.ReportId == reportId);

        report.Should().NotBeNull();
        report.Status.Should().Be(ReportStatus.Paused);
    }

    [Fact]
    public async Task BuildReport_WhenResumed_ShouldContinueFromSavedProgress()
    {
        // arrange
        var (collectionId, sourceFolderPath, _) = await CreateDefaultCollection();
        var exportFolder = Path.Combine(_webApp.TestsTempLocation, "integrity-reports", Guid.NewGuid().ToString());

        var file1 = new FileInfo(
            Path.Combine(_webApp.TestsLocation, "Resources", "file1-5f30f9953332c230d11e3f26db5ae9a0.jpg"));
        var file2 = new FileInfo(
            Path.Combine(_webApp.TestsLocation, "Resources", "file2-09e56a8fd9d1e8beb62c50e6945632bf.jpg"));

        file1.CopyTo(Path.Combine(sourceFolderPath, file1.Name));
        file2.CopyTo(Path.Combine(sourceFolderPath, file2.Name));
        await _mediator.Send(new OverseeCommand());

        var reportId = await _mediator.Send(new CreateIntegrityReportJobCommand([collectionId], exportFolder));
        var buildTask = _reportJobRunner.BuildNextUnfinishedReport(CancellationToken.None);

        await Task.Delay(50);
        await _mediator.Send(new PauseIntegrityReportJobCommand(reportId));
        await buildTask;

        // act
        await _mediator.Send(new ResumeIntegrityReportJobCommand(reportId));
        await BuildReportUntilCompleted(reportId);

        // assert
        _context.ChangeTracker.Clear();
        var report = await _context.IntegrityReports
            .Include(x => x.Collections)
            .FirstOrDefaultAsync(x => x.ReportId == reportId);

        report.Should().NotBeNull();
        report.Status.Should().Be(ReportStatus.Completed);
        report.ProcessedFileCount.Should().Be(2);
    }
}
