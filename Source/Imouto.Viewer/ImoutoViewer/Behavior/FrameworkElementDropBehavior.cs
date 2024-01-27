using System.Windows;
using Microsoft.Xaml.Behaviors;

namespace ImoutoViewer.Behavior;

internal class FrameworkElementDropBehavior : Behavior<FrameworkElement>
{
    private List<string>? _allowDataTypes; //the type of the data that can be dropped into this control

    private IEnumerable<string>? AllowDataTypes
    {
        get
        {
            if (_allowDataTypes == null && (AssociatedObject.DataContext as IDropable) != null)
            {
                _allowDataTypes = ((IDropable)AssociatedObject.DataContext).AllowDataTypes;
            }

            return _allowDataTypes;
        }
    }

    protected override void OnAttached()
    {
        base.OnAttached();

        AssociatedObject.AllowDrop = true;
        AssociatedObject.Drop += AssociatedObject_Drop;
    }

    private void AssociatedObject_Drop(object sender, DragEventArgs e)
    {
        foreach (var item in AllowDataTypes?.Where(item => e.Data.GetDataPresent(item)) ?? [])
        {
            //Drop the data
            if (AssociatedObject.DataContext is IDropable target)
            {
                var data = e.Data.GetData(item);
                
                if (data != null)
                    target.Drop(data, e.Data.GetData("DragSource"));
            }
            break;
        }

        e.Handled = true;
    }
}
