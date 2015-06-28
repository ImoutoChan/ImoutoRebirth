using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using Imouto.Utils;

namespace ImoutoViewer.UserControls
{
    public class BindableTextBlock : TextBlock
    {
        public ObservableCollection<Inline> InlineList
        {
            get { return (ObservableCollection<Inline>)GetValue(InlineListProperty); }
            set { SetValue(InlineListProperty, value); }
        }

        public static readonly DependencyProperty InlineListProperty =
            DependencyProperty.Register("InlineList", typeof(ObservableCollection<Inline>), typeof(BindableTextBlock), new UIPropertyMetadata(null, OnPropertyChanged));

        private static void OnPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            BindableTextBlock textBlock = sender as BindableTextBlock;
            ObservableCollection<Inline> list = e.NewValue as ObservableCollection<Inline>;
            if (list != null)
            {
                list.CollectionChanged += new NotifyCollectionChangedEventHandler(textBlock.InlineCollectionChanged);
                textBlock.Inlines.Clear();
                list.ForEach(x => textBlock.Inlines.Add(x));
            }
        }

        private void InlineCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                this.Inlines.Clear();
                int idx = e.NewItems.Count - 1;
                Inline inline = e.NewItems[idx] as Inline;
                this.Inlines.Add(inline);
            }
        }
    }
}
