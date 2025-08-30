using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using ImoutoRebirth.Navigator.Services.Tags.Model;

namespace ImoutoRebirth.Navigator.Slices.QuickTagging;

internal partial class TagsPacksSetVM : ObservableObject
{
    public ObservableCollection<TagsPackVM> Packs { get; set; } = new();

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Name))]
    public partial string? Title { get; set; }

    public string Name 
        => Title 
           ?? Packs.FirstOrDefault()?.Tags
               .Aggregate("", (x, y) => x.Length > 0 ? x + ", " + y.Title : y.Title) 
           ?? "new";

    public void AddNext(IReadOnlyCollection<Tag> tags)
    {
        var lastKey = Packs.LastOrDefault()?.Key;
        var nextKey = GetNextKey(lastKey);

        if (nextKey == '_')
        {
            Packs.Add(new EmptyTagPackVM());
            nextKey = GetNextKey(nextKey);
        }

        if (nextKey == '-')
        {
            Packs.Add(new EmptyTagPackVM());
            nextKey = GetNextKey(nextKey);
        }

        Packs.Add(new TagsPackVM(tags, nextKey));
    }

    public void Rename(string newTitle) => Title = newTitle;

    private char GetNextKey(char? lastKey)
    {
        if (!lastKey.HasValue)
        {
            return AvailableKeys[0];
        }
        else
        {
            var usedKeys = Packs.Select(x => x.Key).ToHashSet();

            var nextIndex = Array.IndexOf(AvailableKeys, lastKey.Value) + 1;

            if (nextIndex < AvailableKeys.Length && !usedKeys.Contains(AvailableKeys[nextIndex]))
                return AvailableKeys[nextIndex];


            var unused = AvailableKeys.FirstOrDefault(x => !usedKeys.Contains(x));

            if (unused != default)
                return unused;

            return '+';
        }
    }

    private static readonly char[] AvailableKeys = ("12345" + "_WERT" + "ASDFG" + "Z-CVB").ToCharArray();

    public TagsPackVM? GetByKeyOrDefault(char key) 
        => Packs.FirstOrDefault(x => string.Equals(x.Key.ToString(), key.ToString(), StringComparison.OrdinalIgnoreCase));

    public void Clear() => Packs.Clear();

    public void Remove(TagsPackVM pack) => Packs.Remove(pack);
}
