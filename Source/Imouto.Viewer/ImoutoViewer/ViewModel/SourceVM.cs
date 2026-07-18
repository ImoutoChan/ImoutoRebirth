using System.Collections.ObjectModel;
using System.Text;
using System.Windows;
using System.Windows.Input;
using ImoutoRebirth.Common.WPF;
using ImoutoRebirth.Common.WPF.Commands;

namespace ImoutoViewer.ViewModel;

internal class SourceVM : VMBase
{
    private ICommand? _copyTagsAsTextCommand;
    private ICommand? _copyGeneralTagsAsTextCommand;

    public SourceVM(string title) => Title = title;

    public string Title { get; }

    public ObservableCollection<BindedTagVM> TagsCollection { get; } = new ObservableCollection<BindedTagVM>();

    public bool HasGeneralTags => TagsCollection.Any(x => x.Type == "General");

    public ICommand CopyTagsAsTextCommand
        => _copyTagsAsTextCommand ??= new RelayCommand(CopyTagsAsText);

    public ICommand CopyGeneralTagsAsTextCommand
        => _copyGeneralTagsAsTextCommand ??= new RelayCommand(CopyGeneralTagsAsText);

    private void CopyTagsAsText(object? obj)
    {
        var builder = new StringBuilder();

        foreach (var typeGroup in TagsCollection.GroupBy(x => x.Type))
        {
            var tags = string.Join(", ", typeGroup.Select(x => x.Title));
            builder.AppendLine($"{typeGroup.Key}: {tags}");
        }

        SetClipboardText(builder.ToString());
    }

    private void CopyGeneralTagsAsText(object? obj)
    {
        var generalTags = TagsCollection
            .Where(x => x.Type == "General")
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
