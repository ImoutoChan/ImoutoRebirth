using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ImoutoRebirth.Room.WebApi.Client;

namespace ImoutoRebirth.Navigator.Slices.IntegrityReport.ViewModels;

internal partial class IntegrityReportCollectionItemVM : ObservableObject
{
    public IntegrityReportCollectionItemVM(IntegrityReportCollectionResult result)
    {
        CollectionId = result.CollectionId;
        CollectionName = result.CollectionName;
        IsCompleted = result.IsCompleted;
        ExpectedTotalFileCount = result.ExpectedTotalFileCount;
        ProcessedFileCount = result.ProcessedFileCount;
        ReportFiles = result.ReportFiles.ToList();
    }

    public Guid CollectionId { get; }

    public string CollectionName { get; }

    public bool IsCompleted { get; }

    public int ExpectedTotalFileCount { get; }

    public int ProcessedFileCount { get; }

    public IReadOnlyList<string> ReportFiles { get; }

    public bool HasReportFiles => ReportFiles.Count > 0;

    public double ProgressPercent => ExpectedTotalFileCount > 0
        ? (double)ProcessedFileCount / ExpectedTotalFileCount * 100
        : 100;

    public string Status
    {
        get
        {
            if (IsCompleted)
                return "Completed";

            if (ProcessedFileCount == 0)
                return "Pending";

            return "In Progress";
        }
    }

    public string ProgressText => $"{ProcessedFileCount} / {ExpectedTotalFileCount}";

    [RelayCommand]
    private void OpenReportFile(string? filePath)
    {
        if (!string.IsNullOrWhiteSpace(filePath) && System.IO.File.Exists(filePath))
        {
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
            {
                FileName = filePath,
                UseShellExecute = true
            });
        }
    }
}
