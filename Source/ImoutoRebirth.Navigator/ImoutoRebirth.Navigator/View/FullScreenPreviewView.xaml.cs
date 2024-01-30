using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using ImoutoRebirth.Navigator.ViewModel;

namespace ImoutoRebirth.Navigator.View;

public partial class FullScreenPreviewView : UserControl
{
    public FullScreenPreviewView() => InitializeComponent();

    public static readonly DependencyProperty CurrentPreviewControlProperty 
        = DependencyProperty.Register(
            nameof(CurrentPreviewControl), 
            typeof (FrameworkElement), 
            typeof (FullScreenPreviewView), 
            new UIPropertyMetadata(null));

    public FrameworkElement CurrentPreviewControl
    {
        get => (FrameworkElement) GetValue(CurrentPreviewControlProperty);
        set => SetValue(CurrentPreviewControlProperty, value);
    }
    
    private void OnPreviewMouseWheel(object sender, MouseWheelEventArgs e)
    {
        var vm = DataContext as FullScreenPreviewVM;
        
        if (vm?.NextPreviewCommand.CanExecute(e.Delta < 0) == true) 
            vm.NextPreviewCommand.Execute(e.Delta < 0);
    }

    private void ViewPort_OnSizeChanged(object sender, SizeChangedEventArgs e)
    {
        if (e.NewSize.Width == 0 || e.NewSize.Height == 0)
            return;
        
        var senderElement = sender as FrameworkElement;
        var vm = DataContext as FullScreenPreviewVM;
        
        if (senderElement?.Visibility != Visibility.Visible || vm is null)
            return;

        vm.ViewPortSize = e.NewSize;
    }

    private void OnIsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        if (!(bool)e.NewValue) 
            return;

        CurrentPreviewControl = (FrameworkElement)sender;

        var vm = DataContext as FullScreenPreviewVM;
        
        if (vm == null)
            return;

        vm.ViewPortSize = CurrentPreviewControl.RenderSize;
    }
}
