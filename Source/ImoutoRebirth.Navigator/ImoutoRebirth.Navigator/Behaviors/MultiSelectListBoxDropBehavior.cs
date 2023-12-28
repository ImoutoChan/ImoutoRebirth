using System.Windows;
using System.Windows.Controls;

namespace ImoutoRebirth.Navigator.Behaviors;

internal class MultiSelectListBoxDropBehavior : FrameworkElementDropBehavior
{
    protected override void AssociatedObject_DragEnter(object sender, DragEventArgs e)
    {
        if (DataType == null && AssociatedObject is ListBox { DataContext: IDropable dropObject }) 
            DataType = dropObject.DataType;

        e.Handled = true;
    }

    protected override void AssociatedObject_Drop(object sender, DragEventArgs e)
    {
        if (DataType != null && CanBeDropped(e))
        {
            var target = AssociatedObject.DataContext as IDropable;
            var data = e.Data.GetData(DataType);
                
            if (data != null && target != null)
                target.Drop(data);
        }
        e.Handled = true;
    }
}
