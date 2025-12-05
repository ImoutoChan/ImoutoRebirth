using System.Globalization;
using System.Text.Json;
using CsvHelper;
using ImoutoRebirth.Room.Application.Services;
using ImoutoRebirth.Room.Domain.IntegrityAggregate;

namespace ImoutoRebirth.Room.Infrastructure.Service;

internal class IntegrityReportExporter : IIntegrityReportExporter
{
    public async Task<IReadOnlyCollection<string>> ExportReportToFiles(
        IntegrityReportCollection report,
        string exportToFolder)
    {
        Directory.CreateDirectory(exportToFolder);

        var fileName =
            $"Integrity Report {report.CollectionName} (id {report.CollectionId}) {DateTimeOffset.Now:yy-MM-dd-HH-mm}";

        var fullJsonFileName = await WriteJson(report, exportToFolder, fileName);
        var fullCsvFileName = await WriteCsv(report, exportToFolder, fileName);

        return [fullJsonFileName, fullCsvFileName];
    }

    private static async Task<string> WriteCsv(
        IntegrityReportCollection report,
        string reportsDirectory,
        string fileName)
    {
        var reportPath = Path.Combine(reportsDirectory, $"{fileName}.csv");
        await using var writer = new StreamWriter(reportPath);
        await using var csv = new CsvWriter(writer, CultureInfo.InvariantCulture);
        await csv.WriteRecordsAsync(report.CalculatedFileStatuses);

        return reportPath;
    }

    private static async Task<string> WriteJson(
        IntegrityReportCollection report,
        string reportsDirectory,
        string fileName)
    {
        var reportJson = JsonSerializer.Serialize(
            report,
            new JsonSerializerOptions
            {
                WriteIndented = true
            });
        var reportPath = Path.Combine(reportsDirectory, $"{fileName}.json");
        await File.WriteAllTextAsync(reportPath, reportJson);

        return reportPath;
    }
}
