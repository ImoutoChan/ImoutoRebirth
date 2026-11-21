using CommunityToolkit.Mvvm.ComponentModel;

namespace ImoutoRebirth.Navigator.ViewModel.ListEntries;

internal abstract partial class BaseEntryVM(Guid? dbId) : ObservableObject
{
    public Guid? DbId { get; } = dbId;

    public bool IsRatingLoaded { get; private set; }

    [ObservableProperty]
    public partial bool IsFavorite { get; set; }

    [ObservableProperty]
    public partial int Rating { get; set; }

    public void SetRating(bool isFavorite, int rating)
    {
        IsFavorite = isFavorite;
        Rating = rating;
        IsRatingLoaded = true;
    }
}
