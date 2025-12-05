using System.Collections.ObjectModel;
using System.Windows.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ImoutoRebirth.Navigator.Services;
using ImoutoRebirth.Navigator.Services.Collections;
using ImoutoRebirth.Navigator.Slices.IntegrityReport.Services;
using Serilog;

namespace ImoutoRebirth.Navigator.Slices.IntegrityReport.ViewModels;

internal partial class IntegrityReportsVM : ObservableObject
{
    private const int PageSize = 20;

    private readonly IIntegrityReportService _reportService;
    private readonly ICollectionService _collectionService;

    private readonly DispatcherTimer _refreshTimer;

    private int _currentSkip;
    private bool _hasMoreItems = true;
    private bool _isInitialized;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(PauseReportCommand))]
    [NotifyCanExecuteChangedFor(nameof(ResumeReportCommand))]
    public partial IntegrityReportItemVM? SelectedReport { get; set; }

    [ObservableProperty]
    public partial bool IsLoading { get; set; }

    [ObservableProperty]
    public partial bool IsCreatingReport { get; set; }

    [ObservableProperty]
    public partial string? SaveReportPath { get; set; }

    [ObservableProperty]
    public partial bool HasMoreItems { get; set; } = true;

    [ObservableProperty]
    public partial bool IsFlyoutOpen { get; set; }

    public IntegrityReportsVM()
    {
        _reportService = ServiceLocator.GetService<IIntegrityReportService>();
        _collectionService = ServiceLocator.GetService<ICollectionService>();

        _refreshTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(30)
        };
        _refreshTimer.Tick += async (_, _) => await RefreshReportsAsync();
    }

    public ObservableCollection<IntegrityReportItemVM> Reports { get; } = new();

    public ObservableCollection<SelectableCollectionVM> AvailableCollections { get; } = new();

    [RelayCommand]
    private async Task FlyoutOpenChangedAsync()
    {
        IsFlyoutOpen = !IsFlyoutOpen;

        if (IsFlyoutOpen)
        {
            if (!_isInitialized)
            {
                await LoadCollectionsAsync();
                await LoadReportsAsync();
                _isInitialized = true;
            }
            else
            {
                await RefreshReportsAsync();
            }
            _refreshTimer.Start();
        }
        else
        {
            _refreshTimer.Stop();
        }
    }

    // public async Task InitializeAsync()
    // {
    //     if (_isInitialized)
    //         return;
    //
    //     await LoadCollectionsAsync();
    //     await LoadReportsAsync();
    //     _isInitialized = true;
    //     _refreshTimer.Start();
    // }
    //
    // public void StopRefresh()
    // {
    //     _refreshTimer.Stop();
    // }

    private async Task LoadCollectionsAsync()
    {
        try
        {
            var collections = await _collectionService.GetAllCollectionsAsync();
            AvailableCollections.Clear();

            foreach (var collection in collections)
            {
                AvailableCollections.Add(new SelectableCollectionVM(collection.Id, collection.Name, true));
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to load collections for integrity reports");
        }
    }

    [RelayCommand]
    private async Task LoadReportsAsync()
    {
        if (IsLoading)
            return;

        IsLoading = true;
        _currentSkip = 0;
        _hasMoreItems = true;
        HasMoreItems = true;

        try
        {
            var reports = await _reportService.GetReportsAsync(PageSize, 0);

            Reports.Clear();
            foreach (var report in reports)
            {
                Reports.Add(new IntegrityReportItemVM(report));
            }

            _currentSkip = reports.Count;
            _hasMoreItems = reports.Count >= PageSize;
            HasMoreItems = _hasMoreItems;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to load integrity reports");
            App.MainWindowVM?.SetStatusError("Failed to load reports", ex.Message);
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task LoadMoreReportsAsync()
    {
        if (IsLoading || !_hasMoreItems)
            return;

        IsLoading = true;

        try
        {
            var reports = await _reportService.GetReportsAsync(PageSize, _currentSkip);

            foreach (var report in reports)
            {
                Reports.Add(new IntegrityReportItemVM(report));
            }

            _currentSkip += reports.Count;
            _hasMoreItems = reports.Count >= PageSize;
            HasMoreItems = _hasMoreItems;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to load more integrity reports");
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task RefreshReportsAsync()
    {
        if (IsLoading)
            return;

        try
        {
            var reports = await _reportService.GetReportsAsync(Reports.Count > 0 ? Reports.Count : PageSize, 0);

            foreach (var report in reports)
            {
                var existing = Reports.FirstOrDefault(r => r.ReportId == report.ReportId);
                existing?.UpdateFrom(report);
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to refresh integrity reports");
        }
    }

    [RelayCommand]
    private void ShowCreateReport()
    {
        IsCreatingReport = true;
        SelectedReport = null;
        SaveReportPath = null;

        foreach (var collection in AvailableCollections)
            collection.IsSelected = true;
    }

    [RelayCommand]
    private void CancelCreateReport()
    {
        IsCreatingReport = false;
    }

    [RelayCommand]
    private async Task CreateReportAsync()
    {
        IsLoading = true;

        try
        {
            var selectedCollections = AvailableCollections
                .Where(c => c.IsSelected)
                .Select(c => c.Id)
                .ToList();

            IReadOnlyCollection<Guid>? collectionIds = selectedCollections.Count == AvailableCollections.Count
                ? null
                : selectedCollections;

            var savePath = string.IsNullOrWhiteSpace(SaveReportPath) ? null : SaveReportPath;
            var reportId = await _reportService.CreateReportAsync(collectionIds, savePath);

            IsCreatingReport = false;

            await LoadReportsAsync();

            SelectedReport = Reports.FirstOrDefault(r => r.ReportId == reportId);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to create integrity report");
            App.MainWindowVM?.SetStatusError("Failed to create report", ex.Message);
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand(CanExecute = nameof(CanPauseReport))]
    private async Task PauseReportAsync()
    {
        if (SelectedReport == null)
            return;

        try
        {
            await _reportService.PauseReportAsync(SelectedReport.ReportId);

            await RefreshReportsAsync();
            PauseReportCommand.NotifyCanExecuteChanged();
            ResumeReportCommand.NotifyCanExecuteChanged();
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to pause integrity report");
            App.MainWindowVM?.SetStatusError("Failed to pause report", ex.Message);
        }
    }

    private bool CanPauseReport() => SelectedReport?.CanPause == true;

    [RelayCommand(CanExecute = nameof(CanResumeReport))]
    private async Task ResumeReportAsync()
    {
        if (SelectedReport == null)
            return;

        try
        {
            await _reportService.ResumeReportAsync(SelectedReport.ReportId);

            await RefreshReportsAsync();
            PauseReportCommand.NotifyCanExecuteChanged();
            ResumeReportCommand.NotifyCanExecuteChanged();
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to resume integrity report");
            App.MainWindowVM?.SetStatusError("Failed to resume report", ex.Message);
        }
    }

    private bool CanResumeReport() => SelectedReport?.CanResume == true;

    [RelayCommand]
    private void SelectAllCollections()
    {
        foreach (var collection in AvailableCollections)
            collection.IsSelected = true;
    }

    [RelayCommand]
    private void DeselectAllCollections()
    {
        foreach (var collection in AvailableCollections)
            collection.IsSelected = false;
    }
}
