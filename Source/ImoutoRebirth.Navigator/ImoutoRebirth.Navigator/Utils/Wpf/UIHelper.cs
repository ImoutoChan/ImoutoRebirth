﻿using System.Windows;
using System.Windows.Media;

namespace ImoutoRebirth.Navigator.Utils.Wpf;

public static class UIHelper
{
    /// <summary>
    /// Finds a parent of a given item on the visual tree.
    /// </summary>
    /// <typeparam name="T">The type of the queried item.</typeparam>
    /// <param name="child">A direct or indirect child of the queried item.</param>
    /// <returns>The first parent item that matches the submitted type parameter. 
    /// If not matching item can be found, a null reference is being returned.</returns>
    public static T? FindVisualParent<T>(DependencyObject child)
        where T : DependencyObject
    {
        // get parent item
        var parentObject = VisualTreeHelper.GetParent(child);

        return parentObject switch
        {
            // we’ve reached the end of the tree
            null => null,
            // check if the parent matches the type we’re looking for
            T parent => parent,
            _ => FindVisualParent<T>(parentObject)
        };
    }
}
