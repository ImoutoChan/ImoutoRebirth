using System.Windows;
using System.Windows.Input;

namespace ImoutoRebirth.Navigator.Utils.Wpf;

/// <summary>
/// Opens the element's <see cref="FrameworkElement.ContextMenu"/> on a left mouse click,
/// so a context menu can be used as a plain click-to-open dropdown.
/// </summary>
public static class LeftClickContextMenu
{
    public static readonly DependencyProperty EnabledProperty =
        DependencyProperty.RegisterAttached("Enabled", typeof(bool), typeof(LeftClickContextMenu),
            new PropertyMetadata(false, OnEnabledChanged));

    public static bool GetEnabled(DependencyObject d) => (bool)d.GetValue(EnabledProperty);

    public static void SetEnabled(DependencyObject d, bool value) => d.SetValue(EnabledProperty, value);

    private static void OnEnabledChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not FrameworkElement element)
            return;

        if ((bool)e.NewValue)
            element.PreviewMouseLeftButtonUp += OnClick;
        else
            element.PreviewMouseLeftButtonUp -= OnClick;
    }

    private static void OnClick(object sender, MouseButtonEventArgs e)
    {
        if (sender is not FrameworkElement { ContextMenu: { } contextMenu } element)
            return;

        contextMenu.PlacementTarget = element;
        contextMenu.IsOpen = true;
        e.Handled = true;
    }
}
