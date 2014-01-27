using System.Windows;
using System.Windows.Input;
using System.Windows.Interactivity;

namespace ImoutoViewer.Behavior
{
    public class FrameworkElementDragBehavior : Behavior<FrameworkElement>
    {
        private bool isMouseClicked = false;

        protected override void OnAttached()
        {
            base.OnAttached();
            this.AssociatedObject.MouseLeftButtonDown += new MouseButtonEventHandler(AssociatedObject_MouseLeftButtonDown);
            this.AssociatedObject.MouseLeftButtonUp += new MouseButtonEventHandler(AssociatedObject_MouseLeftButtonUp);
            this.AssociatedObject.MouseLeave += new MouseEventHandler(AssociatedObject_MouseLeave);
        }

        void AssociatedObject_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            isMouseClicked = true;
        }

        void AssociatedObject_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            isMouseClicked = false;
        }

        void AssociatedObject_MouseLeave(object sender, MouseEventArgs e)
        {
            if (isMouseClicked)
            {
                //set the item's DataContext as the data to be transferred
                IDragable dragObject = this.AssociatedObject.DataContext as IDragable;
                if (dragObject != null)
                {
                    System.Windows.DragDrop.DoDragDrop(this.AssociatedObject, dragObject.Data, dragObject.AllowDragDropEffects);
                }
            }
            isMouseClicked = false;
        }
    }
}
