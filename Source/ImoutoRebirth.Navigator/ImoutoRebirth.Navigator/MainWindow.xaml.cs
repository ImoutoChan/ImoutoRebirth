using System.Collections;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using ImoutoRebirth.Navigator.Slices.QuickTagging;
using ImoutoRebirth.Navigator.ViewModel;
using ImoutoRebirth.Navigator.ViewModel.ListEntries;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using WpfToolkit.Controls;

namespace ImoutoRebirth.Navigator;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
internal partial class MainWindow
{
    private double _lastExtentHeight = 0;
    private ScrollViewer? _scrollViewerElement;
    private readonly Stack<IReadOnlyCollection<WeakReference<INavigatorListEntry>>> _lastSelectedItems = new();
    private IReadOnlyCollection<WeakReference<INavigatorListEntry>> _currentSelectedItems = [];
    private VirtualizingWrapPanel? _virtualizingWrapPanel;

    public MainWindow()
    {
        InitializeComponent();

        Loaded += (_, _) =>
        {
            ScrollViewerElement.ScrollChanged += (_, _) =>
            {
                var viewModel = DataContext as MainWindowVM;
                viewModel?.LoadPreviewsCommand.Execute(null);
            };

            MediaListBox.SelectionChanged += OnSelectedItemsChanged;

            ScrollViewerElement.ScrollChanged += (_, _) =>
            {
                var scroll = ScrollViewerElement;

                var newExtentHeight = scroll.ExtentHeight;

                var extentHeightChanged = Math.Abs(newExtentHeight - _lastExtentHeight) > double.Epsilon;

                if (extentHeightChanged)
                {
                    var scrollAtTheEnd = scroll.ContentVerticalOffset + scroll.ViewportHeight >= scroll.ScrollableHeight;

                    if (scrollAtTheEnd)
                        scroll.ScrollToEnd();
                }

                _lastExtentHeight = newExtentHeight;
            };
        };
    }

    internal MainWindowVM DataContextVM => (MainWindowVM)DataContext;

    #region Properties

    public IEnumerable<INavigatorListEntry> VisibleItems => GetVisibleItems();

    public IEnumerable<INavigatorListEntry> SelectedEntries
        => MediaListBox.SelectedItems.Cast<INavigatorListEntry>();

    public IList SelectedItems => MediaListBox.SelectedItems;

    public double ViewPortWidth => ScrollViewerElement.ViewportWidth;

    private ScrollViewer ScrollViewerElement => _scrollViewerElement ??= FindFirstVisualChildOfType<ScrollViewer>(MediaListBox)!;

    #endregion Properties

    #region Methods

    private static T? FindFirstVisualChildOfType<T>(DependencyObject? parent)
        where T : class
    {
        if (parent == null)
            return null;

        for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
        {
            var child = VisualTreeHelper.GetChild(parent, i);

            if (child is T childItem)
                return childItem;

            var grandChild = FindFirstVisualChildOfType<T>(child);

            if (grandChild != null)
                return grandChild;
        }
        return null;
    }

    private IReadOnlyList<INavigatorListEntry> GetVisibleItems()
    {
        var itemsHost = _virtualizingWrapPanel ??= MediaListBox.FindChild<VirtualizingWrapPanel>();
        if (itemsHost is null)
            return [];

        var result = new List<INavigatorListEntry>();

        var viewportRect = new Rect(
            new Point(0, 0),
            ScrollViewerElement.RenderSize);

        foreach (var container in itemsHost.Children.OfType<UIElement>())
        {
            if (!IsFullyOrPartiallyVisible(container, ScrollViewerElement, viewportRect))
                continue;

            var item = (INavigatorListEntry)MediaListBox
                .ItemContainerGenerator
                .ItemFromContainer(container);

            result.Add(item);
        }

        return result;
    }

    private static bool IsFullyOrPartiallyVisible(
        UIElement? child,
        UIElement? scrollViewer,
        Rect viewportRectangle)
    {
        if (child == null || scrollViewer == null)
            return false;

        if (child.RenderSize.Width <= 0 || child.RenderSize.Height <= 0)
            return false;

        var childTransform = child.TransformToAncestor(scrollViewer);
        var childRectangle = childTransform.TransformBounds(new Rect(new Point(0, 0), child.RenderSize));

        return viewportRectangle.IntersectsWith(childRectangle);
    }

    public async Task<MessageDialogResult> ShowMessageDialog(
        string title,
        string message,
        MessageDialogStyle style,
        MetroDialogSettings settings)
    {
        MetroDialogOptions.ColorScheme = MetroDialogColorScheme.Accented;

        return await this.ShowMessageAsync(title, message, style, settings);
    }

    public void RevertSelectedItems()
    {
        if (!_lastSelectedItems.TryPop(out var revertTo))
            return;
        
        MediaListBox.SelectionChanged -= OnSelectedItemsChanged;
        
        MediaListBox.SelectedItems.Clear();
        foreach (var item in revertTo)
        {
            if (item.TryGetTarget(out var target))
                MediaListBox.SelectedItems.Add(target);
        }

        MediaListBox.SelectionChanged += OnSelectedItemsChanged;
        
        _currentSelectedItems = SelectedEntries.Select(x => new WeakReference<INavigatorListEntry>(x)).ToList();
        SelectedItemsChanged?.Invoke(this, EventArgs.Empty);
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
            return;

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
            if (item == flyout)
                continue;

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

    private void MainWindow_OnPreviewKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Q && Keyboard.Modifiers.HasFlag(ModifierKeys.Control))
        {
            DataContextVM.ToggleQuickTaggingCommand.Execute(null);
        }

        if (e.Key == Key.Escape)
        {
            foreach (Flyout item in Flyouts.Items)
                item.IsOpen = false;

            if (Visibility == Visibility.Visible && MediaListBox.SelectedIndex >= 0)
            {
                int currentSelectedIndex = MediaListBox.SelectedIndex;
                var selectedItem = MediaListBox.Items[currentSelectedIndex];

                MediaListBox.ScrollIntoView(selectedItem);

                Dispatcher.BeginInvoke(new Action(() =>
                {
                    var item = (UIElement)MediaListBox.ItemContainerGenerator.ContainerFromItem(selectedItem)
                               ?? MediaListBox;
                    item.Focus();

                    MediaListBox.SelectedIndex = currentSelectedIndex;
                }), DispatcherPriority.Render);
            }

        }
    }

    private void MainWindow_OnClosing(object? sender, CancelEventArgs e)
    {
        var viewModel = DataContext as MainWindowVM;
        if (viewModel?.FullScreenPreviewVM == null) 
            return;
        
        viewModel.FullScreenPreviewVM.CloseCommand.Execute(null);
        e.Cancel = true;
    }

    private void QuickTagging_OnIsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        if (Visibility == Visibility.Visible && MediaListBox.SelectedIndex >=0)
        {
            var item = (ListBoxItem)MediaListBox.ItemContainerGenerator.ContainerFromIndex(MediaListBox.SelectedIndex);
            item?.Focus();
        }

    }

    private void MediaListBox_OnKeyUp(object sender, KeyEventArgs e)
    {
        switch (e.Key)
        {
            // Open TagsEdit flyout.
            case Key.T:
                ToggleFlyout(TagsEditFlyout);
                break;
            case Key.M:
                ToggleFlyout(TagsMergeFlyout);
                break;
        }

        Key? toRelease = e.Key switch
        {
            Key.W => Key.Up,
            Key.A => Key.Left,
            Key.S => Key.Down,
            Key.D => Key.Right,
            _ => null
        };

        if (toRelease.HasValue)
            VirtualKeyboard.Release(toRelease.Value);
    }

    private void MediaListBox_OnKeyDown(object sender, KeyEventArgs e)
    {
        Key? toPress = e.Key switch
        {
            Key.W => Key.Up,
            Key.A => Key.Left,
            Key.S => Key.Down,
            Key.D => Key.Right,
            _ => null
        };

        if (toPress.HasValue)
            VirtualKeyboard.Press(toPress.Value);
    }

    #endregion Event handlers

    #region Events

    public event EventHandler? SelectedItemsChanged;

    private void OnSelectedItemsChanged(object _, SelectionChangedEventArgs __)
    {
        _lastSelectedItems.Push(_currentSelectedItems);
        _currentSelectedItems = SelectedEntries.Select(x => new WeakReference<INavigatorListEntry>(x)).ToList();
        
        SelectedItemsChanged?.Invoke(this, EventArgs.Empty);
    }

    #endregion Events
}
