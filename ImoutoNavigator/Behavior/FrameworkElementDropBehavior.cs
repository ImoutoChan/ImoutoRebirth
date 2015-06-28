using System;
using System.Windows;
using System.Windows.Interactivity;

namespace Imouto.Navigator.Behavior
{
    class FrameworkElementDropBehavior : Behavior<FrameworkElement>
    {
        protected Type DataType; //the type of the data that can be dropped into this control

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
            if (DataType != null)
            {
                //if the data type can be dropped 
                if (e.Data.GetDataPresent(DataType))
                {
                    //drop the data
                    IDropable target = AssociatedObject.DataContext as IDropable;
                    target?.Drop(e.Data.GetData(DataType));
                }
            }
            e.Handled = true;
        }

        void AssociatedObject_DragLeave(object sender, DragEventArgs e)
        {
            DataType = null;
            e.Handled = true;
        }

        void AssociatedObject_DragOver(object sender, DragEventArgs e)
        {
            if (DataType != null)
            {
                //if item can be dropped
                if (e.Data.GetDataPresent(DataType))
                {
                    //give mouse effect
                    SetDragDropEffects(e);
                }
            }
            e.Handled = true;
        }

        protected virtual void AssociatedObject_DragEnter(object sender, DragEventArgs e)
        {
            //if the DataContext implements IDropable, record the data type that can be dropped
            if (DataType == null)
            {
                IDropable dropObject = AssociatedObject.DataContext as IDropable;
                if (dropObject != null)
                {
                    DataType = dropObject.DataType;
                }
            }

            e.Handled = true;
        }

        /// <summary>
        /// Provides feedback on if the data can be dropped
        /// </summary>
        /// <param name="e"></param>
        private void SetDragDropEffects(DragEventArgs e)
        {
            e.Effects = DragDropEffects.None;  //default to None

            //if the data type can be dropped 
            if (e.Data.GetDataPresent(DataType))
            {
                e.Effects = DragDropEffects.Copy;
            }
        }

    }
}
