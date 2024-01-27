using System.Windows.Controls;
using System.Windows.Input;
using ImoutoRebirth.Navigator.ViewModel;

namespace ImoutoRebirth.Navigator.View;

public partial class FullScreenPreviewView : UserControl
{
    public FullScreenPreviewView() => InitializeComponent();

    private void OnPreviewMouseWheel(object sender, MouseWheelEventArgs e)
    {
        var vm = DataContext as FullScreenPreviewVM;
        
        if (vm?.NextPreviewCommand?.CanExecute(e.Delta < 0) == true) 
            vm.NextPreviewCommand.Execute(e.Delta < 0);
    }
}
