using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interactivity;

namespace ImageViewer.Behavior
{
    public class FrameworkElementDropBehavior : Behavior<FrameworkElement>
    {
        private List<string> _allowDataTypes; //the type of the data that can be dropped into this control
        private Type _dataType;

        private List<string> AllowDataTypes
        {
            get
            {
                if (_allowDataTypes == null)
                {
                    if (this.AssociatedObject.DataContext as IDropable != null)
                    {
                        _allowDataTypes = (this.AssociatedObject.DataContext as IDropable).AllowDataTypes;
                    }
                }
                return _allowDataTypes;
            }
        }

        protected override void OnAttached()
        {
            base.OnAttached();

            this.AssociatedObject.AllowDrop = true;
            this.AssociatedObject.DragEnter += new DragEventHandler(AssociatedObject_DragEnter);
            this.AssociatedObject.Drop += new DragEventHandler(AssociatedObject_Drop);
        }

        void AssociatedObject_Drop(object sender, DragEventArgs e)
        {
            foreach (var item in AllowDataTypes)
	        {
                if (e.Data.GetDataPresent(item))
                {
                    //drop the data
                    IDropable target = this.AssociatedObject.DataContext as IDropable;
                    target.Drop(e.Data.GetData(item));

                    break;
                }
	        }

            e.Handled = true;
            return;
        }

        void AssociatedObject_DragEnter(object sender, DragEventArgs e)
        {
            e.Effects = DragDropEffects.None;
            //if the DataContext implements IDropable, record the data type that can be dropped
            foreach (var item in AllowDataTypes)
	        {
                if (e.Data.GetDataPresent(item))
                {
                    e.Effects = DragDropEffects.Copy;
                    break;
                }
	        }

            e.Handled = true;
        }
    }
}
