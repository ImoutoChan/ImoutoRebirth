using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using ImoutoRebirth.Room.Application.Cqrs.FoldersSlice;
using ImoutoRebirth.Room.Application.Cqrs.IntegritySlice;
using ImoutoRebirth.Room.Database;
using ImoutoRebirth.Room.Domain.IntegrityAggregate;
using ImoutoRebirth.Room.IntegrationTests.Fixtures;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NodaTime.Serialization.SystemTextJson;
using NodaTime.TimeZones;
using Xunit;

namespace ImoutoRebirth.Room.IntegrationTests;

[Collection("WebApplication")]
public partial class IntegrityReportTests : IDisposable
{
    private readonly TestWebApplicationFactory<Program> _webApp;
    private readonly IServiceScope _scope;
    private readonly RoomDbContext _context;
    private readonly IMediator _mediator;
    private readonly HttpClient _httpClient;
    private readonly IIntegrityReportJobRunner _reportJobRunner;

    public IntegrityReportTests(TestWebApplicationFactory<Program> webApp)
    {
        _webApp = webApp;

        _scope = _webApp.GetScope();
        _context = _webApp.GetDbContext(_scope);
        _mediator = _scope.ServiceProvider.GetRequiredService<IMediator>();
        _httpClient = _webApp.Client;
        _reportJobRunner = _scope.ServiceProvider.GetRequiredService<IIntegrityReportJobRunner>();
    }

    public void Dispose()
    {
        _scope.Dispose();
        _context.Dispose();
        _httpClient.Dispose();
    }

    private async Task<CreatedCollection> CreateDefaultCollection()
    {
        var collectionId = await CreateCollection();
        var collectionPath = Guid.NewGuid().ToString();

        var sourceFolderPath = Path.Combine(_webApp.TestsTempLocation, "collection", collectionPath, "source");
        var destFolderPath = Path.Combine(_webApp.TestsTempLocation, "collection", collectionPath, "dest");

        Directory.CreateDirectory(sourceFolderPath);
        Directory.CreateDirectory(destFolderPath);

        var addSourceFolderCommand = new AddSourceFolderCommand(
            collectionId,
            sourceFolderPath,
            false,
            false,
            false,
            false,
            Array.Empty<string>(),
            false,
            null);

        var addDestinationFolderCommand = new SetDestinationFolderCommand(
            collectionId,
            destFolderPath,
            false,
            false,
            "!format-error",
            "!hash-error",
            "!no-hash-error");

        await _httpClient.PostAsJsonAsync("/collections/source-folders", addSourceFolderCommand);
        await _httpClient.PostAsJsonAsync("/collections/destination-folder", addDestinationFolderCommand);

        return new CreatedCollection(collectionId, sourceFolderPath, destFolderPath);
    }

    private async Task<Guid> CreateCollection()
    {
        var collectionName = $"IntegrityTest Collection {Guid.NewGuid()}";
        var result = await _httpClient.PostAsJsonAsync("/collections", new { name = collectionName });
        var resultString = await result.Content.ReadAsStringAsync();
        return Guid.Parse(resultString[1..^1]);
    }

    private async Task BuildReportUntilCompleted(Guid reportId, int maxIterations = 100)
    {
        for (var i = 0; i < maxIterations; i++)
        {
            await _reportJobRunner.BuildNextUnfinishedReport(CancellationToken.None);

            _context.ChangeTracker.Clear();
            var report = await _context.IntegrityReports.FirstOrDefaultAsync(x => x.ReportId == reportId);

            if (report?.Status == ReportStatus.Completed)
                return;
        }
    }

    private record CreatedCollection(Guid Id, string SourceFolderPath, string DestFolderPath);

    private static JsonSerializerOptions GetJsonOptions()
    {
        var options = new JsonSerializerOptions(JsonSerializerDefaults.Web);
        options.Converters.Add(new JsonStringEnumConverter());
        options.ConfigureForNodaTime(new DateTimeZoneCache(new BclDateTimeZoneSource()));
        return options;
    }
}

