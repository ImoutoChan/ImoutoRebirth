using System.Windows;
using System.Windows.Controls;

namespace ImoutoNavigator.Behavior
{
    class MultiSelectListBoxDropBehavior : FrameworkElementDropBehavior
    {
        protected override void AssociatedObject_DragEnter(object sender, DragEventArgs e)
        {
            //if the DataContext implements IDropable, record the data type that can be dropped
            if (DataType == null)
            {
                var dropObject = (AssociatedObject as ListBox)?.DataContext as IDropable;
                if (dropObject != null)
                {
                    DataType = dropObject.DataType;
                }
            }

            e.Handled = true;
        }

        protected override void AssociatedObject_Drop(object sender, DragEventArgs e)
        {
            if (DataType != null)
            {
                //if the data type can be dropped 
                if (e.Data.GetDataPresent(DataType))
                {
                    //drop the data
                    var target = AssociatedObject.DataContext as IDropable;
                    target?.Drop(e.Data.GetData(DataType));
                }
            }
            e.Handled = true;
        }
    }
}
