using ImoutoRebirth.Lilin.WebApi.Client;
using ImoutoRebirth.Navigator.ViewModel.ListEntries;

namespace ImoutoRebirth.Navigator.Services;

internal class GalleryRatingLoader(FilesClient files)
{
    private CancellationTokenSource? _currentRequest;

    public void Cancel() => _currentRequest?.Cancel();

    public async Task LoadAsync(IReadOnlyCollection<INavigatorListEntry> visible)
    {
        Cancel();

        var pending = visible.Where(x => x.DbId.HasValue && !x.IsRatingLoaded).ToLookup(x => x.DbId!.Value);
        if (pending.Count == 0)
            return;

        using var request = new CancellationTokenSource();
        _currentRequest = request;

        try
        {
            var ratings = await files.GetFileRatingsAsync(new(pending.Select(x => x.Key).ToArray()), request.Token);

            if (request.IsCancellationRequested)
                return;

            var byId = ratings.ToDictionary(x => x.FileId);
            foreach (var group in pending)
            {
                byId.TryGetValue(group.Key, out var rating);
                foreach (var entry in group)
                    entry.SetRating(rating?.IsFavorite ?? false, rating?.Rating ?? 0);
            }
        }
        catch (OperationCanceledException) when (request.IsCancellationRequested)
        {
        }
        finally
        {
            if (_currentRequest == request)
                _currentRequest = null;
        }
    }
}
