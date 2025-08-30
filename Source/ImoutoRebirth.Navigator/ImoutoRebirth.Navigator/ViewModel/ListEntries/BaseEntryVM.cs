using CommunityToolkit.Mvvm.ComponentModel;
using ImoutoRebirth.Lilin.WebApi.Client;

namespace ImoutoRebirth.Navigator.ViewModel.ListEntries;

internal abstract partial class BaseEntryVM : ObservableObject
{
    private static readonly SemaphoreSlim RatingLoaderLocker = new(1);

    private readonly Guid? _dbId;
    private readonly FilesClient _filesClient;
    private bool _isLoaded;

    [ObservableProperty]
    public partial bool IsFavorite { get; set; }

    [ObservableProperty]
    public partial int Rating { get; set; }

    protected BaseEntryVM(Guid? dbId, FilesClient filesClient)
    {
        _dbId = dbId;
        _filesClient = filesClient;
    }

    protected async Task LoadRating()
    {
        if (_dbId == null || _isLoaded)
            return;

        await RatingLoaderLocker.WaitAsync();
        try
        {
            if (_isLoaded)
                return;

            var info = await _filesClient.GetFileInfoAsync(_dbId.Value);

            IsFavorite = info.Tags?.FirstOrDefault(x => x.Tag?.Name == "Favorite") != null;
            var rateValue = info.Tags?.FirstOrDefault(x => x.Tag?.Name == "Rate")?.Value;
            
            if (rateValue is not null && int.TryParse(rateValue, out var rate))
                Rating = rate;

            _isLoaded = true;
        }
        finally
        {
            RatingLoaderLocker.Release();
        }
    }
}
