using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;

namespace ImoutoViewer.UserControls;

public class BindableTextBlock : TextBlock
{
    public ObservableCollection<Inline> InlineList
    {
        get => (ObservableCollection<Inline>)GetValue(InlineListProperty);
        set => SetValue(InlineListProperty, value);
    }

    public static readonly DependencyProperty InlineListProperty =
        DependencyProperty.Register(nameof(InlineList), typeof(ObservableCollection<Inline>), typeof(BindableTextBlock),
            new UIPropertyMetadata(null, OnPropertyChanged));

    private static void OnPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
    {
        var textBlock = sender as BindableTextBlock;
        var list = e.NewValue as ObservableCollection<Inline>;

        if (list != null && textBlock != null)
        {
            list.CollectionChanged += textBlock.InlineCollectionChanged;
            textBlock.Inlines.Clear();
            list.ToList().ForEach(x => textBlock.Inlines.Add(x));
        }
    }

    private void InlineCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.Action != NotifyCollectionChangedAction.Add || e.NewItems == null) 
            return;
        
        Inlines.Clear();
        var idx = e.NewItems.Count - 1;

        if (e.NewItems[idx] is Inline inline)
            Inlines.Add(inline);
    }
}
