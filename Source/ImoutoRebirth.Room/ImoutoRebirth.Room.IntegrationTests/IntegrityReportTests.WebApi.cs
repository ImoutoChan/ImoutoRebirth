using System.Net.Http.Json;
using AwesomeAssertions;
using ImoutoRebirth.Room.Application.Cqrs.IntegritySlice;
using ImoutoRebirth.Room.Domain.IntegrityAggregate;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace ImoutoRebirth.Room.IntegrationTests;

public partial class IntegrityReportTests
{
    [Fact]
    public async Task CreateNewReport_ShouldPersistReportWithCreatedStatus()
    {
        // arrange
        var (collectionId, _, _) = await CreateDefaultCollection();
        var exportFolder = Path.Combine(_webApp.TestsTempLocation, "integrity-reports", Guid.NewGuid().ToString());

        var createCommand = new { ExportToFolder = exportFolder, OnlyCollectionIds = new[] { collectionId } };

        // act
        var response = await _httpClient.PostAsJsonAsync("/integrity-reports", createCommand);
        var reportIdString = await response.Content.ReadAsStringAsync();
        var reportId = Guid.Parse(reportIdString.Trim('"'));

        // assert
        response.EnsureSuccessStatusCode();

        var report = await _context.IntegrityReports
            .Include(x => x.Collections)
            .FirstOrDefaultAsync(x => x.ReportId == reportId);

        report.Should().NotBeNull();
        report.Status.Should().Be(ReportStatus.Created);
        report.ExportToFolder.Should().Be(exportFolder);
        report.Collections.Should().HaveCount(1);
        report.Collections.First().CollectionId.Should().Be(collectionId);
    }

    [Fact]
    public async Task PauseReport_ShouldChangeStatusToPaused()
    {
        // arrange
        var (collectionId, _, _) = await CreateDefaultCollection();
        var exportFolder = Path.Combine(_webApp.TestsTempLocation, "integrity-reports", Guid.NewGuid().ToString());

        var reportId = await _mediator.Send(new CreateIntegrityReportJobCommand([collectionId], exportFolder));

        // act
        var response = await _httpClient.PutAsync($"/integrity-reports/{reportId}/pause", null);

        // assert
        response.EnsureSuccessStatusCode();

        _context.ChangeTracker.Clear();
        var report = await _context.IntegrityReports.FirstOrDefaultAsync(x => x.ReportId == reportId);

        report.Should().NotBeNull();
        report.Status.Should().Be(ReportStatus.Paused);
    }

    [Fact]
    public async Task ResumeReport_ShouldChangeStatusBackToCreated()
    {
        // arrange
        var (collectionId, _, _) = await CreateDefaultCollection();
        var exportFolder = Path.Combine(_webApp.TestsTempLocation, "integrity-reports", Guid.NewGuid().ToString());

        var reportId = await _mediator.Send(new CreateIntegrityReportJobCommand([collectionId], exportFolder));
        await _mediator.Send(new PauseIntegrityReportJobCommand(reportId));

        // act
        var response = await _httpClient.PutAsync($"/integrity-reports/{reportId}/resume", null);

        // assert
        response.EnsureSuccessStatusCode();

        _context.ChangeTracker.Clear();
        var report = await _context.IntegrityReports.FirstOrDefaultAsync(x => x.ReportId == reportId);

        report.Should().NotBeNull();
        report.Status.Should().Be(ReportStatus.Created);
    }

    [Fact]
    public async Task GetReports_ShouldReturnListOfReports()
    {
        // arrange
        var (collectionId, _, _) = await CreateDefaultCollection();
        var exportFolder = Path.Combine(_webApp.TestsTempLocation, "integrity-reports", Guid.NewGuid().ToString());

        var reportId1 = await _mediator.Send(new CreateIntegrityReportJobCommand([collectionId], exportFolder));
        var reportId2 = await _mediator.Send(new CreateIntegrityReportJobCommand([collectionId], exportFolder));

        // act
        var response = await _httpClient.GetAsync("/integrity-reports?count=10&skip=0");

        var reports = await response.Content
            .ReadFromJsonAsync<IReadOnlyCollection<IntegrityReportResult>>(GetJsonOptions());

        // assert
        response.EnsureSuccessStatusCode();
        reports.Should().NotBeNull();
        reports.Count.Should().BeGreaterThanOrEqualTo(2);
        reports.Should().Contain(x => x.ReportId == reportId1);
        reports.Should().Contain(x => x.ReportId == reportId2);
    }

    [Fact]
    public async Task GetReports_ShouldReturnListOfReportsPaged()
    {
        // arrange
        var (collectionId, _, _) = await CreateDefaultCollection();
        var exportFolder = Path.Combine(_webApp.TestsTempLocation, "integrity-reports", Guid.NewGuid().ToString());

        var reportId1 = await _mediator.Send(new CreateIntegrityReportJobCommand([collectionId], exportFolder));
        var reportId2 = await _mediator.Send(new CreateIntegrityReportJobCommand([collectionId], exportFolder));

        // act
        var response = await _httpClient.GetAsync("/integrity-reports?count=10&skip=1");
        var reports = await response.Content
            .ReadFromJsonAsync<IReadOnlyCollection<IntegrityReportResult>>(GetJsonOptions());

        // assert
        response.EnsureSuccessStatusCode();
        reports.Should().NotBeNull();
        reports.Should().NotContain(x => x.ReportId == reportId2);
        reports.Should().Contain(x => x.ReportId == reportId1);
    }
}
