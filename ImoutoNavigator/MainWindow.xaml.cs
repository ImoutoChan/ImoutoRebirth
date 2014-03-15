using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using ImoutoNavigator.ViewModel;

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
                        viewModel.LoadPreviewsCommand.Execute(null);
                };
            };
        }

        public IEnumerable VisibleItems
        {
            get
            {
                if (ScrollViewerElement != null)
                {

                    var result = new List<ImageEntryVM>();

                    result.AddRange(
                        from ImageEntryVM item in ListBoxElement.Items
                        let listBoxItem = (FrameworkElement) ListBoxElement.ItemContainerGenerator.ContainerFromItem(item) 
                        where IsFullyOrPartiallyVisible(listBoxItem, ScrollViewerElement) 
                        select item);

                    return result;
                }
                else
                {
                    return null;
                }
            }
        }

        private ScrollViewer ScrollViewerElement
        {
            get { return FindFirstVisualChildOfType<ScrollViewer>(ListBoxElement); }
        }

        private T FindFirstVisualChildOfType<T>(DependencyObject parent)
            where T : class
        {
            if (parent == null) return null;

            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(parent, i);
                T childT = child as T;
                if (childT != null)
                {
                    return childT;
                }
                else
                {
                    T grandChild = FindFirstVisualChildOfType<T>(child);
                    if (grandChild != null)
                    {
                        return grandChild;
                    }
                }
            }
            return null;
        }

        private bool IsFullyOrPartiallyVisible(FrameworkElement child, FrameworkElement scrollViewer)
        {
            var childTransform = child.TransformToAncestor(scrollViewer);
            var childRectangle = childTransform.TransformBounds(new Rect(new Point(0, 0), child.RenderSize));
            var ownerRectangle = new Rect(new Point(0, 0), scrollViewer.RenderSize);
            return ownerRectangle.IntersectsWith(childRectangle);
        }
    }
}
