using System.Collections.ObjectModel;
using System.Text.Json;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ImoutoRebirth.Common;

namespace ImoutoRebirth.Navigator.Slices.QuickTagging;

internal partial class AvailableTagPacksSetsVM : ObservableObject
{
    public ObservableCollection<TagsPacksSetVM> Sets { get; } = new();

    [ObservableProperty]
    public partial TagsPacksSetVM Selected { get; set; }

    public AvailableTagPacksSetsVM()
    {
        Sets.Add(new TagsPacksSetVM());
        Selected = Sets.First();

        Load();
    }

    public void SaveSets()
    {
        var serialized = JsonSerializer.Serialize(Sets);
        Clipboard.SetText(serialized);
        Settings.Default.SavedTagPacks = serialized;
        Settings.Default.Save();
    }

    public void DeleteSelected()
    {
        Sets.Remove(Selected);

        if (Sets.None())
            Sets.Add(new TagsPacksSetVM());

        Selected = Sets.First();
    }

    private void Load()
    {
        var settings = Settings.Default.SavedTagPacks;

        if (string.IsNullOrWhiteSpace(settings)) 
            return;

        var savedSets = JsonSerializer.Deserialize<ObservableCollection<TagsPacksSetVM>>(settings)!;

        Sets.Clear();

        foreach (var set in savedSets) 
            Sets.Add(set);
    }

    [RelayCommand]
    private void SelectNext()
    {
        var index = Sets.IndexOf(Selected);
        Selected = index == Sets.Count - 1 ? Sets.First() : Sets[index + 1];
    }
}
