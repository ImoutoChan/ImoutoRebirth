using ImoutoRebirth.Lilin.WebApi.Client;

namespace ImoutoRebirth.Navigator.ViewModel.ListEntries;

internal abstract class BaseEntryVM : VMBase
{
    private static readonly SemaphoreSlim RatingLoaderLocker = new(1);

    private readonly Guid? _dbId;
    private readonly IImoutoRebirthLilinWebApiClient _lilinWebApiClient;

    private bool _isFavorite;
    private int _rating;
    private bool _isLoaded = false;

    protected BaseEntryVM(Guid? dbId, IImoutoRebirthLilinWebApiClient lilinWebApiClient)
    {
        _dbId = dbId;
        _lilinWebApiClient = lilinWebApiClient;
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

            var info = await _lilinWebApiClient.Files.GetFileInfoAsync(_dbId.Value);

            IsFavorite = info.Tags.FirstOrDefault(x => x.Tag.Name == "Favorite") != null;
            var rateValue = info.Tags.FirstOrDefault(x => x.Tag.Name == "Rate")?.Value;
            if (rateValue is not null && int.TryParse(rateValue, out var rate))
                Rating = rate;

            _isLoaded = true;
        }
        finally
        {
            RatingLoaderLocker.Release();
        }
    }

    public bool IsFavorite
    {
        get => _isFavorite;
        private set => OnPropertyChanged(ref _isFavorite, value, () => IsFavorite);
    }

    public int Rating
    {
        get => _rating;
        private set => OnPropertyChanged(ref _rating, value, () => Rating);
    }
}