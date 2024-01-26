using System.Windows;
using System.Windows.Documents;

namespace ImoutoRebirth.Navigator.Utils.Wpf;

public static class RunVisibility
{
    public static readonly DependencyProperty VisibleProperty =
        DependencyProperty.RegisterAttached("Visible", typeof(bool), typeof(RunVisibility),
            new FrameworkPropertyMetadata(true, OnVisibilityChanged));

    public static bool GetVisible(Run d)
    {
        return (bool)d.GetValue(VisibleProperty);
    }

    public static void SetVisible(Run d, bool value)
    {
        d.SetValue(VisibleProperty, value);
    }

    private static void OnVisibilityChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not Run run) 
            return;

        if ((bool)e.NewValue)
        {
            if (run.Tag != null)
                run.FontSize = (double)run.Tag;
        }
        else
        {
            run.Tag = run.FontSize;
            run.FontSize = 0.004;
        }
    }
}
