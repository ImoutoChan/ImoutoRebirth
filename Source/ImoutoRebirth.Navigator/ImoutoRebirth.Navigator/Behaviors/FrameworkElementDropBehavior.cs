using System.Windows;
using Microsoft.Xaml.Behaviors;

namespace ImoutoRebirth.Navigator.Behaviors;

internal class FrameworkElementDropBehavior : Behavior<FrameworkElement>
{
    /// <summary>
    /// The type of the data that can be dropped into this control
    /// </summary>
    protected Type? DataType;

    protected override void OnAttached()
    {
        base.OnAttached();

        AssociatedObject.AllowDrop = true;
        AssociatedObject.DragEnter += AssociatedObject_DragEnter;
        AssociatedObject.DragOver += AssociatedObject_DragOver;
        AssociatedObject.DragLeave += AssociatedObject_DragLeave;
        AssociatedObject.Drop += AssociatedObject_Drop;
    }

    protected virtual void AssociatedObject_Drop(object sender, DragEventArgs e)
    {
        if (DataType != null && CanBeDropped(e))
        {
            var target = AssociatedObject.DataContext as IDropable;
            var data = e.Data.GetData(DataType);
                
            if (data != null)
                target?.Drop(data);
        }
        e.Handled = true;
    }

    private void AssociatedObject_DragLeave(object sender, DragEventArgs e)
    {
        DataType = null;
        e.Handled = true;
    }

    private void AssociatedObject_DragOver(object sender, DragEventArgs e)
    {
        if (DataType != null && CanBeDropped(e)) 
            SetDragDropEffects(e);

        e.Handled = true;
    }

    protected virtual void AssociatedObject_DragEnter(object sender, DragEventArgs e)
    {
        // if the DataContext implements IDropable, record the data type that can be dropped
        if (DataType == null && AssociatedObject.DataContext is IDropable dropObject)
            DataType = dropObject.DataType;

        e.Handled = true;
    }

    /// <summary>
    /// Provides feedback on if the data can be dropped
    /// </summary>
    private void SetDragDropEffects(DragEventArgs e)
    {
        e.Effects = DragDropEffects.None;

        if (CanBeDropped(e)) 
            e.Effects = DragDropEffects.Copy;
    }

    protected bool CanBeDropped(DragEventArgs e) => e.Data.GetDataPresent(DataType);
}
