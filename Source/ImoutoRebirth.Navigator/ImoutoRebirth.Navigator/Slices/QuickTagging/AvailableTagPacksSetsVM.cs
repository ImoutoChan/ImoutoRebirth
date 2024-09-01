using System.Collections.ObjectModel;
using System.Text.Json;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using ImoutoRebirth.Common;
using ImoutoRebirth.Navigator.Services.Tags.Model;

namespace ImoutoRebirth.Navigator.Slices.QuickTagging;

internal partial class AvailableTagPacksSetsVM : ObservableObject
{
    public ObservableCollection<TagsPacksSetVM> Sets { get; } = new();

    [ObservableProperty]
    private TagsPacksSetVM _selected;

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

        //foreach (var savedSet in savedSets.Where(x => x.Packs.Any(y => y.Any())))
        //{
        //    var set = new TagsPacksSetVM();
        //    set.
        //    foreach (var pack in savedSet.Packs.Where(x => x.Any())) 
        //        set.AddNext(pack);

        //    Sets.Add(set);
        //}
    }
}
