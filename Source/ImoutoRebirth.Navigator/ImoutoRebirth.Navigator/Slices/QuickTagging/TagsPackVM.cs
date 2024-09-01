using CommunityToolkit.Mvvm.ComponentModel;
using ImoutoRebirth.Navigator.Services.Tags.Model;

namespace ImoutoRebirth.Navigator.Slices.QuickTagging;

internal partial class TagsPackVM : ObservableObject
{
    [ObservableProperty]
    private IReadOnlyCollection<Tag> _tags;

    [ObservableProperty]
    private char _key;

    [ObservableProperty]
    private bool? _isSuccess = null;

    public TagsPackVM(IReadOnlyCollection<Tag> tags, char key)
    {
        _tags = tags;
        _key = key;
    }
}

internal class EmptyTagPackVM : TagsPackVM
{
    public EmptyTagPackVM() : base([], '_')
    {
    }
}
