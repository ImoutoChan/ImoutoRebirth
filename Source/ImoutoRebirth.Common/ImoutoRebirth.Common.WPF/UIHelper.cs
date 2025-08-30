using System.Windows;
using System.Windows.Media;

namespace ImoutoRebirth.Common.WPF;

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

    public static T? FindVisualChild<T>(DependencyObject? parent, string? withName) 
        where T : DependencyObject
    {
        if (parent == null) 
            return null;

        var childrenCount = VisualTreeHelper.GetChildrenCount(parent);

        for (var i = 0; i < childrenCount; i++)
        {
            var child = VisualTreeHelper.GetChild(parent, i);

            if (child is not T childType)
            {
                var foundChild = FindVisualChild<T>(child, withName);

                if (foundChild != null) 
                    return foundChild;
            }
            else if (!string.IsNullOrEmpty(withName))
            {
                if (childType is FrameworkElement frameworkElement && frameworkElement.Name == withName)
                {
                    return childType;
                }
                else
                {
                    var foundChild = FindVisualChild<T>(childType, withName);
                    
                    if (foundChild != null) 
                        return foundChild;
                }
            }
            else
            {
                return childType;
            }
        }

        return null;
    }

    public static T? GetParentOfType<T>(this DependencyObject? child) where T : DependencyObject
    {
        while (child != null)
        {
            if (child is T t)
                return t;

            child = VisualTreeHelper.GetParent(child);
        }

        return null;
    }
}
