using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace ImoutoRebirth.Navigator.View.Flyouts;

/// <summary>
///     Interaction logic for TagsEditView.xaml
/// </summary>
public partial class TagsEditView : UserControl
{
    public TagsEditView()
    {
        InitializeComponent();
    }

    private void TagsEditView_OnIsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        if (IsVisible)
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                SearchTagTextBox.Focus();
            }), DispatcherPriority.Render);
        }
    }
}
