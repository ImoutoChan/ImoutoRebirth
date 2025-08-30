using CommunityToolkit.Mvvm.ComponentModel;
using ImoutoRebirth.Navigator.Services.Tags.Model;

namespace ImoutoRebirth.Navigator.Slices.QuickTagging;

internal partial class TagsPackVM : ObservableObject
{
    [ObservableProperty]
    public partial IReadOnlyCollection<Tag> Tags { get; set; }

    [ObservableProperty]
    public partial char Key { get; set; }

    [ObservableProperty]
    public partial bool? IsSuccess { get; set; } = null;

    public TagsPackVM(IReadOnlyCollection<Tag> tags, char key)
    {
        Tags = tags;
        Key = key;
    }
}

internal class EmptyTagPackVM : TagsPackVM
{
    public EmptyTagPackVM() : base([], '_')
    {
    }
}
