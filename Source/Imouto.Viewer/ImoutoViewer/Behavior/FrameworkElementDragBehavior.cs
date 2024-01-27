using System.Windows;
using System.Windows.Input;
using Microsoft.Xaml.Behaviors;

namespace ImoutoViewer.Behavior;

internal class FrameworkElementDragBehavior : Behavior<FrameworkElement>
{
    private bool _isMouseClicked;

    protected override void OnAttached()
    {
        base.OnAttached();
        AssociatedObject.MouseLeftButtonDown += AssociatedObject_MouseLeftButtonDown;
        AssociatedObject.MouseLeftButtonUp += AssociatedObject_MouseLeftButtonUp;
        AssociatedObject.MouseLeave += AssociatedObject_MouseLeave;
    }

    private void AssociatedObject_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) => _isMouseClicked = true;

    private void AssociatedObject_MouseLeftButtonUp(object sender, MouseButtonEventArgs e) => _isMouseClicked = false;

    private void AssociatedObject_MouseLeave(object sender, MouseEventArgs e)
    {
        if (_isMouseClicked)
        {
            // Set the item's DataContext as the data to be transferred
            if (AssociatedObject.DataContext is IDragable { Data: not null } dragObject)
            {
                DragDrop.DoDragDrop(AssociatedObject, dragObject.Data, dragObject.AllowDragDropEffects);
            }
        }
        _isMouseClicked = false;
    }
}
