using System.Collections;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using ImoutoRebirth.Navigator.ViewModel;
using ImoutoRebirth.Navigator.ViewModel.ListEntries;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;

namespace ImoutoRebirth.Navigator;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
internal partial class MainWindow : MetroWindow
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

            ListBoxElement.SelectionChanged += (o, eventArgs) => OnSelectedItemsChanged();
        };
    }

    #region Properties

    public IEnumerable<INavigatorListEntry> VisibleItems
    {
        get
        {
            if (ScrollViewerElement == null)
            {
                return null;
            }

            return 
                from INavigatorListEntry item in ListBoxElement.Items
                let listBoxItem = (UIElement)ListBoxElement.ItemContainerGenerator.ContainerFromItem(item)
                where IsFullyOrPartiallyVisible(listBoxItem, ScrollViewerElement)
                select item;
        }
    }

    public IEnumerable<INavigatorListEntry> SelectedEntries 
        => ListBoxElement.SelectedItems.Cast<INavigatorListEntry>();

    public IList SelectedItems => ListBoxElement.SelectedItems;

    private ScrollViewer ScrollViewerElement
    {
        get { return FindFirstVisualChildOfType<ScrollViewer>(ListBoxElement); }
    }

    #endregion Properties

    #region Methods

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
        if (child == null || scrollViewer == null)
        {
            return false;
        }

        var childTransform = child.TransformToAncestor(scrollViewer);
        var childRectangle = childTransform.TransformBounds(new Rect(new Point(0, 0), child.RenderSize));
        var ownerRectangle = new Rect(new Point(0, 0), scrollViewer.RenderSize);
        return ownerRectangle.IntersectsWith(childRectangle);
    }

    public async Task<MessageDialogResult> ShowMessageDialog(string title, string message, MessageDialogStyle style, MetroDialogSettings settings)
    {
        MetroDialogOptions.ColorScheme = MetroDialogColorScheme.Accented;

        return await this.ShowMessageAsync(title, message, style, settings);
    }

    #endregion Methods

    #region Event handlers

    /// <summary>
    /// Open (sender.Tag) flyout.
    /// </summary>
    private void Button_Click(object sender, RoutedEventArgs e)
    {
        var flyout = sender as Flyout ?? (sender as FrameworkElement)?.Tag as Flyout;
        if (flyout == null)
        {
            return;
        }

        ToggleFlyout(flyout);
    }

    /// <summary>
    /// Change flyout IsOpen state.
    /// </summary>
    /// <param name="flyout">Flyout to change.</param>
    private void ToggleFlyout(Flyout flyout)
    {
        foreach (Flyout item in Flyouts.Items)
        {
            item.IsOpen = false;
        }

        flyout.IsOpen = !flyout.IsOpen;
    }

    /// <summary>
    ///     Close all flyouts.
    /// </summary>
    private void Grid_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
        foreach (Flyout item in Flyouts.Items)
        {
            if (Equals(item, TagsEditFlyout))
            {
                continue;
            }

            if (item.IsOpen)
            {
                e.Handled = true;
            }
            item.IsOpen = false;
        }
        if (e.Handled)
        {
            Client.Focus();
        }
    }

    private void UIElement_OnKeyUp(object sender, KeyEventArgs e)
    {
        // Open TagsEdit flyout.
        if (e.Key == Key.T)
        {
            ToggleFlyout(TagsEditFlyout);
        }
    }

    #endregion Event handlers

    #region Events

    public event EventHandler SelectedItemsChanged;

    private void OnSelectedItemsChanged()
    {
        var handler = SelectedItemsChanged;
        handler?.Invoke(this, new EventArgs());
    }

    #endregion Events
}