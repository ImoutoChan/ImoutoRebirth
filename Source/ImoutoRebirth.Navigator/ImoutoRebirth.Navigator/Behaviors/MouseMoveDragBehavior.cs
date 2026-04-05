using System.Windows;
using System.Windows.Input;
using Microsoft.Xaml.Behaviors;
using Serilog;

namespace ImoutoRebirth.Navigator.Behaviors;

internal class MouseMoveDragBehavior : Behavior<FrameworkElement>
{
    private bool _isMouseClicked;
    private Point _mouseDownPosition;

    protected override void OnAttached()
    {
        base.OnAttached();
        AssociatedObject.PreviewMouseLeftButtonDown += OnPreviewMouseLeftButtonDown;
        AssociatedObject.MouseLeftButtonUp += OnMouseLeftButtonUp;
        AssociatedObject.MouseMove += OnMouseMove;
    }

    protected override void OnDetaching()
    {
        base.OnDetaching();
        AssociatedObject.PreviewMouseLeftButtonDown -= OnPreviewMouseLeftButtonDown;
        AssociatedObject.MouseLeftButtonUp -= OnMouseLeftButtonUp;
        AssociatedObject.MouseMove -= OnMouseMove;
    }

    private void OnPreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        _isMouseClicked = true;
        _mouseDownPosition = e.GetPosition(AssociatedObject);
    }

    private void OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e) => _isMouseClicked = false;

    private void OnMouseMove(object sender, MouseEventArgs e)
    {
        if (!_isMouseClicked || e.LeftButton != MouseButtonState.Pressed)
        {
            _isMouseClicked = false;
            return;
        }

        var currentPosition = e.GetPosition(AssociatedObject);
        var diff = _mouseDownPosition - currentPosition;

        if (Math.Abs(diff.X) <= SystemParameters.MinimumHorizontalDragDistance &&
            Math.Abs(diff.Y) <= SystemParameters.MinimumVerticalDragDistance)
            return;

        _isMouseClicked = false;

        if (AssociatedObject.DataContext is IDragable dragObject)
        {
            try
            {
                DragDrop.DoDragDrop(AssociatedObject, dragObject.Data, dragObject.AllowDragDropEffects);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error during drag and drop");
            }
        }
    }
}

