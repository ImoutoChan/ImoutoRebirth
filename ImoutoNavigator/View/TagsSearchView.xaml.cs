using System.Windows.Controls;
using System.Windows.Input;
using Imouto.Navigator.ViewModel;

namespace Imouto.Navigator.View
{
    /// <summary>
    /// Interaction logic for TagsSearchView.xaml
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
                (this.DataContext as TagSearchVM).SelectTagCommand.Execute(HintListBox.SelectedItem);
                e.Handled = true;
            }
        }
    }
}
