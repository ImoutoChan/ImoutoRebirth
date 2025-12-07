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
    public async Task BuildReport_WithValidFiles_ShouldCompleteSuccessfully()
    {
        // arrange
        var (collectionId, sourceFolderPath, _) = await CreateDefaultCollection();
        var exportFolder = Path.Combine(_webApp.TestsTempLocation, "integrity-reports", Guid.NewGuid().ToString());

        var file = new FileInfo(Path.Combine(_webApp.TestsLocation, "Resources", "file1-5f30f9953332c230d11e3f26db5ae9a0.jpg"));
        file.CopyTo(Path.Combine(sourceFolderPath, file.Name));
        await _mediator.Send(new OverseeCommand());

        var reportId = await _mediator.Send(new CreateIntegrityReportJobCommand([collectionId], exportFolder));

        // act
        await BuildReportUntilCompleted(reportId);

        // assert
        _context.ChangeTracker.Clear();
        var report = await _context.IntegrityReports
            .Include(x => x.Collections)
            .FirstOrDefaultAsync(x => x.ReportId == reportId);

        report.Should().NotBeNull();
        report.Status.Should().Be(ReportStatus.Completed);
        report.ProcessedFileCount.Should().Be(1);
        report.ExpectedTotalFileCount.Should().Be(1);
        report.Collections.First().IsCompleted.Should().BeTrue();

        Directory.Exists(exportFolder).Should().BeTrue();
        Directory.GetFiles(exportFolder, "*.json").Should().HaveCount(1);
        Directory.GetFiles(exportFolder, "*.csv").Should().HaveCount(1);
    }

    [Fact]
    public async Task BuildReport_WithMissingFile_ShouldMarkFileAsMissing()
    {
        // arrange
        var (collectionId, sourceFolderPath, destFolderPath) = await CreateDefaultCollection();
        var exportFolder = Path.Combine(_webApp.TestsTempLocation, "integrity-reports", Guid.NewGuid().ToString());

        var file = new FileInfo(Path.Combine(_webApp.TestsLocation, "Resources", "file1-5f30f9953332c230d11e3f26db5ae9a0.jpg"));
        file.CopyTo(Path.Combine(sourceFolderPath, file.Name));
        await _mediator.Send(new OverseeCommand());

        var destFiles = Directory.GetFiles(destFolderPath, "*", SearchOption.AllDirectories);
        foreach (var destFile in destFiles)
            File.Delete(destFile);

        var reportId = await _mediator.Send(new CreateIntegrityReportJobCommand([collectionId], exportFolder));

        // act
        await BuildReportUntilCompleted(reportId);

        // assert
        _context.ChangeTracker.Clear();
        var report = await _context.IntegrityReports
            .Include(x => x.Collections)
            .FirstOrDefaultAsync(x => x.ReportId == reportId);

        report.Should().NotBeNull();
        report.Status.Should().Be(ReportStatus.Completed);

        var jsonFiles = Directory.GetFiles(exportFolder, "*.json");
        jsonFiles.Should().HaveCount(1);

        var jsonContent = await File.ReadAllTextAsync(jsonFiles[0]);
        jsonContent.Should().Contain("\"IsPresent\": false");
    }

    [Fact]
    public async Task BuildReport_WithCorruptedFile_ShouldMarkFileWithHashMismatch()
    {
        // arrange
        var (collectionId, sourceFolderPath, destFolderPath) = await CreateDefaultCollection();
        var exportFolder = Path.Combine(_webApp.TestsTempLocation, "integrity-reports", Guid.NewGuid().ToString());

        var file = new FileInfo(Path.Combine(_webApp.TestsLocation, "Resources", "file1-5f30f9953332c230d11e3f26db5ae9a0.jpg"));
        file.CopyTo(Path.Combine(sourceFolderPath, file.Name));
        await _mediator.Send(new OverseeCommand());

        var destFiles = Directory.GetFiles(destFolderPath, "*", SearchOption.AllDirectories);
        destFiles.Should().HaveCount(1);

        await File.AppendAllTextAsync(destFiles[0], "corrupted data");

        var reportId = await _mediator.Send(new CreateIntegrityReportJobCommand([collectionId], exportFolder));

        // act
        await BuildReportUntilCompleted(reportId);

        // assert
        _context.ChangeTracker.Clear();
        var report = await _context.IntegrityReports
            .Include(x => x.Collections)
            .FirstOrDefaultAsync(x => x.ReportId == reportId);

        report.Should().NotBeNull();
        report.Status.Should().Be(ReportStatus.Completed);

        var jsonFiles = Directory.GetFiles(exportFolder, "*.json");
        jsonFiles.Should().HaveCount(1);

        var jsonContent = await File.ReadAllTextAsync(jsonFiles[0]);
        jsonContent.Should().Contain("\"IsPresent\": true");
        jsonContent.Should().NotContain("\"ActualHash\": \"5f30f9953332c230d11e3f26db5ae9a0\"");
    }

    [Fact]
    public async Task BuildReport_WithMultipleCollections_ShouldProcessAllCollections()
    {
        // arrange
        var (collectionId1, sourceFolderPath1, _) = await CreateDefaultCollection();
        var (collectionId2, sourceFolderPath2, _) = await CreateDefaultCollection();
        var exportFolder = Path.Combine(_webApp.TestsTempLocation, "integrity-reports", Guid.NewGuid().ToString());

        var file1 = new FileInfo(Path.Combine(_webApp.TestsLocation, "Resources", "file1-5f30f9953332c230d11e3f26db5ae9a0.jpg"));
        var file2 = new FileInfo(Path.Combine(_webApp.TestsLocation, "Resources", "file2-09e56a8fd9d1e8beb62c50e6945632bf.jpg"));

        file1.CopyTo(Path.Combine(sourceFolderPath1, file1.Name));
        file2.CopyTo(Path.Combine(sourceFolderPath2, file2.Name));
        await _mediator.Send(new OverseeCommand());

        var reportId = await _mediator.Send(new CreateIntegrityReportJobCommand(
            [collectionId1, collectionId2],
            exportFolder));

        // act
        await BuildReportUntilCompleted(reportId);

        // assert
        _context.ChangeTracker.Clear();
        var report = await _context.IntegrityReports
            .Include(x => x.Collections)
            .FirstOrDefaultAsync(x => x.ReportId == reportId);

        report.Should().NotBeNull();
        report.Status.Should().Be(ReportStatus.Completed);
        report.Collections.Should().HaveCount(2);
        report.Collections.Should().OnlyContain(x => x.IsCompleted);
        report.ExpectedTotalFileCount.Should().Be(2);
        report.ProcessedFileCount.Should().Be(2);

        Directory.GetFiles(exportFolder, "*.json").Should().HaveCount(2);
        Directory.GetFiles(exportFolder, "*.csv").Should().HaveCount(2);
    }

    [Fact]
    public async Task BuildReport_ShouldExportJsonAndCsvFiles()
    {
        // arrange
        var (collectionId, sourceFolderPath, _) = await CreateDefaultCollection();
        var exportFolder = Path.Combine(_webApp.TestsTempLocation, "integrity-reports", Guid.NewGuid().ToString());

        var file = new FileInfo(Path.Combine(_webApp.TestsLocation, "Resources", "file1-5f30f9953332c230d11e3f26db5ae9a0.jpg"));
        file.CopyTo(Path.Combine(sourceFolderPath, file.Name));
        await _mediator.Send(new OverseeCommand());

        var reportId = await _mediator.Send(new CreateIntegrityReportJobCommand([collectionId], exportFolder));

        // act
        await BuildReportUntilCompleted(reportId);

        // assert
        Directory.Exists(exportFolder).Should().BeTrue();

        var jsonFiles = Directory.GetFiles(exportFolder, "*.json");
        var csvFiles = Directory.GetFiles(exportFolder, "*.csv");

        jsonFiles.Should().HaveCount(1);
        csvFiles.Should().HaveCount(1);

        var jsonContent = await File.ReadAllTextAsync(jsonFiles[0]);
        jsonContent.Should().Contain("CollectionId");
        jsonContent.Should().Contain("CollectionName");
        jsonContent.Should().Contain("CalculatedFileStatuses");

        var csvContent = await File.ReadAllTextAsync(csvFiles[0]);
        csvContent.Should().NotBeEmpty();
        csvContent.Should().Contain("FileId");
    }
}
