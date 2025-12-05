using ImoutoRebirth.Room.Database.Entities;
using ImoutoRebirth.Room.Domain.IntegrityAggregate;

namespace ImoutoRebirth.Room.DataAccess.Mappers;

internal static class IntegrityReportMapper
{
    public static IntegrityReportEntity ToEntity(this IntegrityReport report)
    {
        var reportEntity = new IntegrityReportEntity
        {
            ReportId = report.ReportId,
            StartedOn = report.StartedOn,
            Status = report.Status,
            ExpectedTotalFileCount = report.ExpectedTotalFileCount,
            ProcessedFileCount = report.ProcessedFileCount,
            ExportToFolder = report.ExportToFolder
        };

        reportEntity.Collections = report.Collections
            .Select(collection => collection.ToEntity(reportEntity.ReportId))
            .ToList();

        return reportEntity;
    }

    public static void UpdateCollections(
        this IntegrityReportEntity entity,
        IntegrityReport report)
    {
        foreach (var collection in report.Collections)
        {
            var collectionEntity = entity.Collections.Single(x => x.CollectionId == collection.CollectionId);
            collectionEntity.IsCompleted = collection.IsCompleted;
            collectionEntity.ExpectedTotalFileCount = collection.ExpectedTotalFileCount;
            collectionEntity.ProcessedFileCount = collection.ProcessedFileCount;
            collectionEntity.ReportExportedFiles = collection.ReportExportedFiles;
        }

        entity.Status = report.Status;
        entity.ExpectedTotalFileCount = report.ExpectedTotalFileCount;
        entity.ProcessedFileCount = report.ProcessedFileCount;
    }

    private static IntegrityReportCollectionEntity ToEntity(
        this IntegrityReportCollection integrityReportCollection,
        Guid reportId)
        => new()
        {
            ReportId = reportId,
            CollectionId = integrityReportCollection.CollectionId,
            CollectionName = integrityReportCollection.CollectionName,
            IsCompleted = integrityReportCollection.IsCompleted,
            ExpectedTotalFileCount = integrityReportCollection.ExpectedTotalFileCount,
            ProcessedFileCount = integrityReportCollection.ProcessedFileCount,
            ReportExportedFiles = integrityReportCollection.ReportExportedFiles,
            FileStatuses = integrityReportCollection.CalculatedFileStatuses
                .Select(x => x.ToEntity(reportId, integrityReportCollection.CollectionId))
                .ToList()
        };

    public static IntegrityReportFileStatusEntity ToEntity(
        this IntegrityReportFileStatus status,
        Guid reportId,
        Guid collectionId)
        => new()
        {
            ReportId = reportId,
            CollectionId = collectionId,
            FileId = status.FileId,
            ExpectedFullPath = status.ExpectedFullPath,
            FileName = status.FileName,
            IsPresent = status.IsPresent,
            ExpectedHash = status.ExpectedHash,
            ActualHash = status.ActualHash,
            ReadingProblem = status.ReadingProblem,
            Status = status.Status
        };

    public static IntegrityReport ToModel(this IntegrityReportEntity entity)
        => IntegrityReport.Restore(
            entity.ReportId,
            entity.StartedOn,
            entity.Status,
            entity.ExpectedTotalFileCount,
            entity.ProcessedFileCount,
            entity.ExportToFolder,
            entity.Collections.Select(ToModel).ToList());

    private static IntegrityReportCollection ToModel(this IntegrityReportCollectionEntity entity)
        => IntegrityReportCollection.Restore(
            entity.CollectionId,
            entity.CollectionName,
            entity.FileStatuses.Select(ToModel).ToList(),
            entity.IsCompleted,
            entity.ExpectedTotalFileCount,
            entity.ProcessedFileCount,
            entity.ReportExportedFiles);

    private static IntegrityReportFileStatus ToModel(this IntegrityReportFileStatusEntity entity)
        => IntegrityReportFileStatus.Restore(
            entity.FileId,
            entity.ExpectedFullPath,
            entity.FileName,
            entity.IsPresent,
            entity.ExpectedHash,
            entity.ActualHash,
            entity.ReadingProblem);
}
