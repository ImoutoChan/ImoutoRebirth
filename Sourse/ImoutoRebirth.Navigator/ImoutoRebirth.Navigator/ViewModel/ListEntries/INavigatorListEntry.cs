using System.Windows;
using System.Windows.Media.Imaging;
using ImoutoRebirth.Navigator.Behavior;

namespace ImoutoRebirth.Navigator.ViewModel.ListEntries;

internal interface INavigatorListEntry : IDragable
{
    void Load();

    void UpdatePreview(Size previewSize);

    ListEntryType Type { get; }

    Size ViewPortSize { get; }

    Guid? DbId { get; }

    string Path { get; }

    bool IsLoading { get; }

    BitmapSource Image { get; }

    bool IsFavorite { get; }

    int Rating { get; }
}