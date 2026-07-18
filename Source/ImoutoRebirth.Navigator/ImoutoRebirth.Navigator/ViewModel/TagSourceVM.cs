using System.Collections.ObjectModel;
using System.Text;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace ImoutoRebirth.Navigator.ViewModel;

internal partial class TagSourceVM : ObservableObject
{
    public TagSourceVM(string title, ObservableCollection<BindedTagVM> tags)
    {
        Title = title;
        Tags = tags;
    }

    public string Title { get; private set; }

    public ObservableCollection<BindedTagVM> Tags { get; private set; }

    public bool HasGeneralTags => Tags.Any(x => x.Tag.Type.Title == "General");

    [RelayCommand]
    private void CopyTagsAsText()
    {
        var builder = new StringBuilder();

        foreach (var typeGroup in Tags.GroupBy(x => x.Tag.Type.Title))
        {
            var tags = string.Join(", ", typeGroup.Select(x => x.Title));
            builder.AppendLine($"{typeGroup.Key}: {tags}");
        }

        SetClipboardText(builder.ToString());
    }

    [RelayCommand]
    private void CopyGeneralTagsAsText()
    {
        var generalTags = Tags
            .Where(x => x.Tag.Type.Title == "General")
            .Select(x => x.Title);

        SetClipboardText(string.Join(", ", generalTags));
    }

    private static void SetClipboardText(string text)
    {
        text = text.TrimEnd();

        if (!string.IsNullOrEmpty(text))
            Clipboard.SetText(text);
    }
}
