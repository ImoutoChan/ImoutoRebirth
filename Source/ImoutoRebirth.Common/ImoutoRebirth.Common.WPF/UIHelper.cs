using System.Windows;
using System.Windows.Media;

namespace ImoutoRebirth.Common.WPF;

public static class UIHelper
{
    /// <summary>
    /// Finds a parent of a given item on the visual tree.
    /// </summary>
    /// <typeparam name="TType">The type of the queried item.</typeparam>
    /// <param name="child">A direct or indirect child of the queried item.</param>
    /// <returns>The first parent item that matches the submitted type parameter. 
    /// If not matching item can be found, a null reference is being returned.</returns>
    public static TType? FindVisualParent<TType>(DependencyObject child)
        where TType : DependencyObject
    {
        // get parent item
        var parentObject = VisualTreeHelper.GetParent(child);

        return parentObject switch
        {
            // we’ve reached the end of the tree
            null => null,
            // check if the parent matches the type we’re looking for
            TType parent => parent,
            _ => FindVisualParent<TType>(parentObject)
        };
    }

    public static TType? FindVisualChild<TType>(DependencyObject? parent, string? withName)
        where TType : DependencyObject
    {
        if (parent == null) 
            return null;

        var childrenCount = VisualTreeHelper.GetChildrenCount(parent);

        for (var i = 0; i < childrenCount; i++)
        {
            var child = VisualTreeHelper.GetChild(parent, i);

            if (child is not TType childType)
            {
                var foundChild = FindVisualChild<TType>(child, withName);

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
                    var foundChild = FindVisualChild<TType>(childType, withName);
                    
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

    public static TType? GetParentOfType<TType>(this DependencyObject? child) where TType : DependencyObject
    {
        while (child != null)
        {
            if (child is TType t)
                return t;

            child = VisualTreeHelper.GetParent(child);
        }

        return null;
    }
}
