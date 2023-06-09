﻿using System.Diagnostics;
using System.Windows;
using System.Windows.Input;
using Microsoft.Xaml.Behaviors;

namespace ImoutoRebirth.Navigator.Behavior;

class FrameworkElementDragBehavior : Behavior<FrameworkElement>
{
    protected bool IsMouseClicked { get; set; }

    protected override void OnAttached()
    {
        base.OnAttached();
        AssociatedObject.PreviewMouseLeftButtonDown += AssociatedObject_MouseLeftButtonDown;
        AssociatedObject.MouseLeftButtonUp += AssociatedObject_MouseLeftButtonUp;
        AssociatedObject.MouseLeave += AssociatedObject_MouseLeave;
    }

    void AssociatedObject_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        IsMouseClicked = true;
    }

    void AssociatedObject_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
        IsMouseClicked = false;
    }

    protected virtual void AssociatedObject_MouseLeave(object sender, MouseEventArgs e)
    {
        if (IsMouseClicked)
        {
            // Set the item's DataContext as the data to be transferred
            if (AssociatedObject.DataContext is IDragable dragObject)
            {
                try
                {
                    DragDrop.DoDragDrop(AssociatedObject, dragObject.Data, dragObject.AllowDragDropEffects);
                }
                catch (Exception exception)
                {
                    Debug.WriteLine(exception);
                }
            }
        }
        IsMouseClicked = false;
    }
}