using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using ImoutoRebirth.Navigator.ViewModel;

namespace ImoutoRebirth.Navigator.View;

/// <summary>
///     Interaction logic for TagsSearchView.xaml
/// </summary>
public partial class TagsSearchView : UserControl
{
    public TagsSearchView()
    {
        InitializeComponent();
    }

    private void TextBox_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Down)
        {
            if (HintListBox.SelectedIndex < HintListBox.Items.Count - 1)
            {
                HintListBox.SelectedIndex = HintListBox.SelectedIndex + 1;
            }
            else
            {
                HintListBox.SelectedIndex = 0;
            }

            e.Handled = true;
        }
        else if (e.Key == Key.Up)
        {
            if (HintListBox.SelectedIndex > 0)
            {
                HintListBox.SelectedIndex = HintListBox.SelectedIndex - 1;
            }
            else
            {
                HintListBox.SelectedIndex = HintListBox.Items.Count - 1;
            }

            e.Handled = true;
        }
        else if (e.Key == Key.Enter)
        {
            ((TagSearchVM)DataContext).SelectTagCommand.Execute(HintListBox.SelectedItem);
            e.Handled = true;
        }
    }

    private void EnteredValueTextBox_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Down)
        {
            if (ValuesHintListBox.SelectedIndex < ValuesHintListBox.Items.Count - 1)
            {
                ValuesHintListBox.SelectedIndex += 1;
            }
            else
            {
                ValuesHintListBox.SelectedIndex = 0;
            }

            e.Handled = true;
        }
        else if (e.Key == Key.Up)
        {
            if (ValuesHintListBox.SelectedIndex > 0)
            {
                ValuesHintListBox.SelectedIndex -= 1;
            }
            else
            {
                ValuesHintListBox.SelectedIndex = ValuesHintListBox.Items.Count - 1;
            }

            e.Handled = true;
        }
        else if (e.Key == Key.Enter)
        {
            ((TagSearchVM)DataContext).SelectTagValueCommand.Execute(ValuesHintListBox.SelectedItem);
            e.Handled = true;
        }
    }

    private void EnterValueGrid_OnIsVisibleChanged(object _, DependencyPropertyChangedEventArgs e)
    {
        if (e.NewValue is not true) 
            return;

        Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Input, () =>
        {
            TagValuesSearchTextBox.Focus();
            Keyboard.Focus(TagValuesSearchTextBox);
            TagValuesSearchTextBox.SelectAll();
        });
    }
}
