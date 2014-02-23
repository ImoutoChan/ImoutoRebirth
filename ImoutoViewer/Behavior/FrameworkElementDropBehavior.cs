using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interactivity;
using ImoutoViewer.UserControls;

namespace ImoutoViewer.Behavior
{
    public class FrameworkElementDropBehavior : Behavior<FrameworkElement>
    {
        private List<string> _allowDataTypes; //the type of the data that can be dropped into this control

        private IEnumerable<string> AllowDataTypes
        {
            get
            {
                if (_allowDataTypes == null && (AssociatedObject.DataContext as IDropable) != null)
                {
                    _allowDataTypes = (AssociatedObject.DataContext as IDropable).AllowDataTypes;
                }

                return _allowDataTypes;
            }
        }

        protected override void OnAttached()
        {
            base.OnAttached();

            AssociatedObject.AllowDrop = true;
            //AssociatedObject.DragEnter += AssociatedObject_DragEnter;
            AssociatedObject.Drop += AssociatedObject_Drop;
        }

        void AssociatedObject_Drop(object sender, DragEventArgs e)
        {
            foreach (var item in AllowDataTypes.Where(item => e.Data.GetDataPresent(item)))
	        {
	            //Drop the data
	            var target = AssociatedObject.DataContext as IDropable;

	            if (target != null)
	            {
                    target.Drop(e.Data.GetData(item), e.Data.GetData("DragSource"));
	            }
	            break;
	        }

            e.Handled = true;
        }

        //void AssociatedObject_DragEnter(object sender, DragEventArgs e)
        //{
        //    e.Effects = DragDropEffects.None;

        //    //if the DataContext implements IDropable, record the data type that can be dropped
        //    if (AllowDataTypes.Any(item => e.Data.GetDataPresent(item)))
        //    {
        //        e.Effects = DragDropEffects.Copy;
        //    }

        //    e.Handled = true;
        //}
    }
}
