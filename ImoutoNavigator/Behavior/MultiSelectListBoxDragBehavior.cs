using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using ImoutoNavigator.ViewModel;

namespace ImoutoNavigator.Behavior
{
    class MultiSelectListBoxDragBehavior : FrameworkElementDragBehavior
    {
        protected override void AssociatedObject_MouseLeave(object sender, MouseEventArgs e)
        {
            if (IsMouseClicked)
            {
                // Set the item's DataContext as the data to be transferred
                var dragObject = (AssociatedObject.Tag as ListBox)?.SelectedItems.Cast<BindedTagVM>().ToList();
                if (dragObject != null)
                {
                    DragDrop.DoDragDrop(AssociatedObject, dragObject, DragDropEffects.Copy);
                }
            }
            IsMouseClicked = false;
        }
    }
}
