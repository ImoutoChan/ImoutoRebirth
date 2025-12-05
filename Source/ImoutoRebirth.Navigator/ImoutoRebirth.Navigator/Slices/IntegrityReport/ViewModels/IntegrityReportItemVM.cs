using System.Collections.ObjectModel;
using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ImoutoRebirth.Common;
using ImoutoRebirth.Room.WebApi.Client;

namespace ImoutoRebirth.Navigator.Slices.IntegrityReport.ViewModels;

internal partial class IntegrityReportItemVM : ObservableObject
{
    private DateTimeOffset? _lastUpdateTime;
    private int _lastProcessedCount;

    [ObservableProperty]
    public partial ReportStatus Status { get; set; }

    [ObservableProperty]
    public partial int ProcessedFileCount { get; set; }

    [ObservableProperty]
    public partial int ExpectedTotalFileCount { get; set; }

    [ObservableProperty]
    public partial string? EstimatedTimeRemaining { get; set; }

    public IntegrityReportItemVM(IntegrityReportResult result)
    {
        ReportId = result.ReportId;
        StartedOn = result.StartedOn;
        Status = result.Status;
        ExpectedTotalFileCount = result.ExpectedTotalFileCount;
        ProcessedFileCount = result.ProcessedFileCount;
        ExportToFolder = result.ExportToFolder;

        FillCollections(result);

        _lastProcessedCount = ProcessedFileCount;
        _lastUpdateTime = DateTimeOffset.UtcNow;
    }

    public Guid ReportId { get; }

    public DateTimeOffset StartedOn { get; }

    public string StartedOnFormatted => StartedOn.ToString("dd MMM yyyy HH:mm", CultureInfo.InvariantCulture);

    public string ExportToFolder { get; }

    public bool HasExportToFolder => !string.IsNullOrWhiteSpace(ExportToFolder);

    public ObservableCollection<IntegrityReportCollectionItemVM> Collections { get; } = new();

    public string CollectionNames => Collections.Select(c => c.CollectionName).JoinStrings(", ");

    public double ProgressPercent => ExpectedTotalFileCount > 0
        ? (double)ProcessedFileCount / ExpectedTotalFileCount * 100
        : 100;

    public string ProgressText => $"{ProcessedFileCount} / {ExpectedTotalFileCount}";

    public bool IsCompleted => Status == ReportStatus.Completed;

    public bool IsPaused => Status == ReportStatus.Paused;

    public bool IsBuilding => Status is ReportStatus.Building or ReportStatus.Created;

    public bool CanPause => Status is ReportStatus.Building or ReportStatus.Created;

    public bool CanResume => Status == ReportStatus.Paused;

    [RelayCommand]
    private void OpenReportFolder()
    {
        if (!string.IsNullOrWhiteSpace(ExportToFolder) && System.IO.Directory.Exists(ExportToFolder))
        {
            System.Diagnostics.Process.Start("explorer.exe", ExportToFolder);
        }
    }

    public void UpdateFrom(IntegrityReportResult result)
    {
        var now = DateTimeOffset.UtcNow;

        Status = result.Status;

        // Calculate ETA before updating ProcessedFileCount
        if (IsBuilding && _lastUpdateTime.HasValue && result.ProcessedFileCount > _lastProcessedCount)
        {
            var elapsedSeconds = (now - _lastUpdateTime.Value).TotalSeconds;
            var processedDelta = result.ProcessedFileCount - _lastProcessedCount;
            var remainingFiles = result.ExpectedTotalFileCount - result.ProcessedFileCount;

            if (processedDelta > 0 && elapsedSeconds > 0)
            {
                var filesPerSecond = processedDelta / elapsedSeconds;
                var remainingSeconds = remainingFiles / filesPerSecond;
                var eta = TimeSpan.FromSeconds(remainingSeconds);

                if (eta.TotalHours >= 1)
                {
                    EstimatedTimeRemaining = $"{(int)eta.TotalHours}h {eta.Minutes}m remaining";
                }
                else if (eta.TotalMinutes >= 1)
                {
                    EstimatedTimeRemaining = $"{eta.Minutes}m {eta.Seconds}s remaining";
                }
                else
                {
                    EstimatedTimeRemaining = $"{eta.Seconds}s remaining";
                }
            }
        }
        else if (!IsBuilding)
        {
            EstimatedTimeRemaining = null;
        }

        ProcessedFileCount = result.ProcessedFileCount;
        ExpectedTotalFileCount = result.ExpectedTotalFileCount;

        _lastProcessedCount = ProcessedFileCount;
        _lastUpdateTime = now;

        Collections.Clear();

        FillCollections(result);

        OnPropertyChanged(nameof(ProgressPercent));
        OnPropertyChanged(nameof(ProgressText));
        OnPropertyChanged(nameof(IsCompleted));
        OnPropertyChanged(nameof(IsPaused));
        OnPropertyChanged(nameof(IsBuilding));
        OnPropertyChanged(nameof(CanPause));
        OnPropertyChanged(nameof(CanResume));
    }

    private void FillCollections(IntegrityReportResult result)
    {
        var collections = result.Collections
            .Select(x => new IntegrityReportCollectionItemVM(x))
            .OrderBy(x => x.Status)
            .ThenBy(x => x.CollectionName);

        foreach (var collection in collections)
            Collections.Add(collection);
    }
}
