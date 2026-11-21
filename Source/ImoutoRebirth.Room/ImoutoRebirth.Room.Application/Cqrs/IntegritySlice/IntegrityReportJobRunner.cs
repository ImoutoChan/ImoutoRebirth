using System.Security.Cryptography;
using System.Threading.Channels;
using ImoutoRebirth.Common.Jobs;
using ImoutoRebirth.Common.Jobs.Progress;
using ImoutoRebirth.Room.Application.Cqrs.CollectionFileSlice;
using ImoutoRebirth.Room.Application.Services;
using ImoutoRebirth.Room.Domain;
using ImoutoRebirth.Room.Domain.IntegrityAggregate;
using MediatR;
using Microsoft.Extensions.Logging;

namespace ImoutoRebirth.Room.Application.Cqrs.IntegritySlice;

public interface IIntegrityReportJobRunner
{
    Task BuildNextUnfinishedReport(CancellationToken ct);
}

internal class IntegrityReportJobRunner : IIntegrityReportJobRunner
{
    private readonly IMediator _mediator;
    private readonly IIntegrityReportExporter _integrityReportExporter;
    private readonly ILoggerFactory _loggerFactory;
    private readonly IIntegrityReportRepository _integrityReportRepository;
    private readonly ILogger<IntegrityReportJobRunner> _logger;
    private readonly IRunningJobs<RoomJobType> _runningJobs;

    public IntegrityReportJobRunner(
        IMediator mediator,
        IIntegrityReportExporter integrityReportExporter,
        ILoggerFactory loggerFactory,
        IIntegrityReportRepository integrityReportRepository,
        ILogger<IntegrityReportJobRunner> logger,
        IRunningJobs<RoomJobType> runningJobs)
    {
        _mediator = mediator;
        _integrityReportExporter = integrityReportExporter;
        _loggerFactory = loggerFactory;
        _integrityReportRepository = integrityReportRepository;
        _logger = logger;
        _runningJobs = runningJobs;
    }

    public async Task BuildNextUnfinishedReport(CancellationToken ct)
    {
        RunningJob<RoomJobType>? job = null;
        try
        {
            var unfinished = await _integrityReportRepository.GetFirstUnfinished();
            if (unfinished is null)
                return;

            job = new (RoomJobType.IntegrityReportBuilder, unfinished.ReportId, new CancellationTokenSource());
            _runningJobs.Add(job);

            using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(ct, job.Interruptable.Token);
            await BuildReport(unfinished, linkedCts.Token);
        }
        catch (OperationCanceledException e) when (job?.Interruptable.IsCancellationRequested == true)
        {
            _logger.LogInformation(e, "Integrity report building was paused by user");
        }
        catch (Exception e)
        {
            _logger.LogWarning(e, "An error occurred while building integrity report");
        }
        finally
        {
            if (job != null)
            {
                _runningJobs.Interrupt(job.JobType, job.Id);
                job.Interruptable.Dispose();
            }
        }
    }

    private async Task BuildReport(IntegrityReport report, CancellationToken ct)
    {
        await FillExpectedTotalFilesInCollections(report, ct);
        await UpdateReportStatus(report, ReportStatus.Building);

        foreach (var collection in report.Collections.Where(x => !x.IsCompleted))
        {
            var allCollectionFiles = await GetAllCollectionFiles(collection, ct);

            var alreadyProcessedFiles = collection.GetAlreadyProcessedFileIds();
            var filesToProcess = allCollectionFiles.Where(x => !alreadyProcessedFiles.Contains(x.Id)).ToList();

            report.UpdateExpectedTotalFileCount(collection.CollectionId, allCollectionFiles.Count);
            await _integrityReportRepository.Update(report);

            using var progress = new LoggerProgressReporter(_loggerFactory, filesToProcess.Count);

            var parallelCount = Math.Max(Environment.ProcessorCount / 2, 1);

            var filesToCheckChannel = Channel.CreateUnbounded<CollectionFile>();
            var checkedFilesToSaveChannel = Channel.CreateBounded<IntegrityReportFileStatus>(parallelCount * 100);

            foreach (var file in filesToProcess)
                filesToCheckChannel.Writer.TryWrite(file);

            filesToCheckChannel.Writer.Complete();

            var fileStatusChecker = async () =>
            {
                while (filesToCheckChannel.Reader.TryRead(out var file))
                {
                    var status = await CheckFileStatus(file, ct);
                    await checkedFilesToSaveChannel.Writer.WriteAsync(status, ct);
                }
            };

            var fileStatusCheckersAll = async () =>
            {
                do
                {
                    await Task.WhenAll(
                    [
                        ..Enumerable.Range(0, parallelCount).Select(_ => fileStatusChecker())
                    ]);
                } while (filesToCheckChannel.Reader.Count > 0);

                checkedFilesToSaveChannel.Writer.TryComplete();
            };

            var checkedFilesPersistence = async () =>
            {
                var statusesBatch = new List<IntegrityReportFileStatus>(parallelCount * 100);
                var chunksProcessed = 0;
                try
                {
                    while (await checkedFilesToSaveChannel.Reader.WaitToReadAsync(ct))
                    {
                        while (statusesBatch.Count < parallelCount * 100
                               && checkedFilesToSaveChannel.Reader.TryRead(out var status))
                        {
                            statusesBatch.Add(status);
                        }

                        await _integrityReportRepository.ReportFiles(
                            statusesBatch,
                            report.ReportId,
                            collection.CollectionId);

                        foreach (var status in statusesBatch)
                        {
                            report.SaveFileStatus(collection.CollectionId, status);
                            progress.ReportItemProcessed();
                        }

                        if (++chunksProcessed % 10 == 0)
                            await _integrityReportRepository.Update(report);

                        statusesBatch.Clear();
                    }
                }
                catch (Exception e)
                {
                    checkedFilesToSaveChannel.Writer.TryComplete(e);
                    throw;
                }
            };

            await Task.WhenAll(fileStatusCheckersAll(), checkedFilesPersistence());

            var checkedIds = collection.GetAlreadyProcessedFileIds().ToHashSet();
            if (allCollectionFiles.Any(file => !checkedIds.Contains(file.Id)))
                throw new InvalidOperationException("Integrity results are incomplete.");

            IReadOnlyCollection<string> savedToPaths;
            try
            {
                savedToPaths = await _integrityReportExporter.ExportReportToFiles(collection, report.ExportToFolder);
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or ArgumentException or NotSupportedException)
            {
                _logger.LogError(
                    ex,
                    "Pausing integrity report {ReportId} after export failed in {Folder}",
                    report.ReportId,
                    report.ExportToFolder);
                await UpdateReportStatus(report, ReportStatus.Paused);
                return;
            }

            report.SetCollectionCompleted(collection.CollectionId, savedToPaths);
            await _integrityReportRepository.Update(report);
        }

        await UpdateReportStatus(report, ReportStatus.Completed);
        await _integrityReportRepository.DeleteFiles(report.ReportId);
    }

    private async Task UpdateReportStatus(IntegrityReport report, ReportStatus status)
    {
        report.SetStatus(status);
        await _integrityReportRepository.Update(report);
    }

    private async Task<IReadOnlyCollection<CollectionFile>> GetAllCollectionFiles(
        IntegrityReportCollection collection,
        CancellationToken ct) => await _mediator.Send(
        new CollectionFilesModelsQuery(
            new CollectionFilesQuery(
                CollectionId: collection.CollectionId,
                CollectionFileIds: [],
                Path: null,
                Md5: [],
                Count: null,
                Skip: null)),
        ct);

    private async Task FillExpectedTotalFilesInCollections(IntegrityReport report, CancellationToken ct)
    {
        foreach (var collection in report.Collections.Where(x => x.ExpectedTotalFileCount == 0))
        {
            var filesCount = await CountFilesInCollection(collection, ct);
            report.UpdateExpectedTotalFileCount(collection.CollectionId, filesCount);
            await _integrityReportRepository.Update(report);
        }
    }

    private async Task<int> CountFilesInCollection(IntegrityReportCollection collection, CancellationToken ct)
        => await _mediator.Send(
            new CollectionFilesCountQuery(
                new CollectionFilesQuery(
                    CollectionId: collection.CollectionId,
                    CollectionFileIds: [],
                    Path: null,
                    Md5: [],
                    Count: null,
                    Skip: null)),
            ct);

    private static async Task<IntegrityReportFileStatus> CheckFileStatus(CollectionFile file, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        var expectedFullPath = file.Path;

        string actualHash;
        try
        {
            using var md5 = MD5.Create();
            await using var stream = File.OpenRead(expectedFullPath);
            var computed = await md5.ComputeHashAsync(stream, ct);
            actualHash = Convert.ToHexStringLower(computed);
        }
        catch (Exception e)
        {
            return IntegrityReportFileStatus.Create(file, false, null, e.GetType().Name + ":" + e.Message);
        }

        return IntegrityReportFileStatus.Create(file, true, actualHash, null);
    }
}
