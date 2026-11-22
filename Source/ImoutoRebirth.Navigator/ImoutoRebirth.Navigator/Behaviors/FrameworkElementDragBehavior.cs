using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using ImoutoRebirth.Common.WPF;
using Microsoft.Xaml.Behaviors;
using Serilog;

namespace ImoutoRebirth.Navigator.Behaviors;

internal class FrameworkElementDragBehavior : Behavior<FrameworkElement>
{
    protected bool IsMouseClicked { get; set; }

    protected override void OnAttached()
    {
        base.OnAttached();
        AssociatedObject.PreviewMouseLeftButtonDown += AssociatedObject_MouseLeftButtonDown;
        AssociatedObject.MouseLeftButtonUp += AssociatedObject_MouseLeftButtonUp;
        AssociatedObject.MouseLeave += AssociatedObject_MouseLeave;
    }

    private void AssociatedObject_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        => IsMouseClicked = e.OriginalSource is not Visual source || source.GetParentOfType<Slider>() is null;

    private void AssociatedObject_MouseLeftButtonUp(object sender, MouseButtonEventArgs e) => IsMouseClicked = false;

    protected virtual void AssociatedObject_MouseLeave(object sender, MouseEventArgs e)
    {
        if (!IsMouseClicked) 
            return;
        
        // Set the item's DataContext as the data to be transferred
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

        IsMouseClicked = false;
    }
}
