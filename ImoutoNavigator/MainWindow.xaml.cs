using ImoutoNavigator.ViewModel;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using MahApps.Metro.Controls;

namespace ImoutoNavigator
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        public MainWindow()
        {
            InitializeComponent();

            Loaded += (sender, args) =>
            {
                ScrollViewerElement.ScrollChanged += (o, eventArgs) =>
                {
                    var viewModel = DataContext as MainWindowVM;
                   
                    if (viewModel.LoadPreviewsCommand.CanExecute(null))
                    {
                        viewModel.LoadPreviewsCommand.Execute(null);
                    }
                };
            };
        }

        public IEnumerable VisibleItems
        {
            get
            {
                if (ScrollViewerElement == null)
                {
                    return null;
                }
                
                var result = new List<ImageEntryVM>();

                result.AddRange(
                                from ImageEntryVM item in ListBoxElement.Items
                                let listBoxItem =
                                    (FrameworkElement) ListBoxElement.ItemContainerGenerator.ContainerFromItem(item)
                                where IsFullyOrPartiallyVisible(listBoxItem, ScrollViewerElement)
                                select item);

                return result;
            }
        }

        private ScrollViewer ScrollViewerElement
        {
            get { return FindFirstVisualChildOfType<ScrollViewer>(ListBoxElement); }
        }

        private static T FindFirstVisualChildOfType<T>(DependencyObject parent)
            where T : class
        {
            if (parent == null)
            {
                return null;
            }

            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);

                if (child is T)
                {
                    return child as T;
                }
                else
                {
                    var grandChild = FindFirstVisualChildOfType<T>(child);
                    if (grandChild != null)
                    {
                        return grandChild;
                    }
                }
            }
            return null;
        }

        private static bool IsFullyOrPartiallyVisible(UIElement child, UIElement scrollViewer)
        {
            if (child == null ||
                scrollViewer == null)
            {
                return false;
            }

            var childTransform = child.TransformToAncestor(scrollViewer);
            var childRectangle = childTransform.TransformBounds(new Rect(new Point(0, 0), child.RenderSize));
            var ownerRectangle = new Rect(new Point(0, 0), scrollViewer.RenderSize);
            return ownerRectangle.IntersectsWith(childRectangle);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {

            foreach (Flyout item in Flyouts.Items)
            {
                item.IsOpen = false;
            }
            CollectionsFlyOut.IsOpen = !CollectionsFlyOut.IsOpen;
        }
    }
}
