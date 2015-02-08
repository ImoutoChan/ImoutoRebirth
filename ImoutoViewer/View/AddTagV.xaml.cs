using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ImoutoViewer.View
{
    /// <summary>
    /// Interaction logic for AddTagV.xaml
    /// </summary>
    public partial class AddTagV : UserControl
    {
        private bool _shiftPressed;

        public AddTagV()
        {
            InitializeComponent();
        }

        private void KeyDownEventHandler (object sender, KeyEventArgs e)
        {
            if (e.Key == Key.LeftShift || e.Key == Key.RightShift)
            {
                _shiftPressed = true;
            }

            if (e.Key >= Key.A && e.Key <= Key.Z)
            {
                var letter = e.Key.ToString();
                letter = _shiftPressed ? letter.ToUpper() : letter.ToLower();

                TextCompositionManager.StartComposition(new TextComposition(InputManager.Current, sender as TextBox, letter));

                e.Handled = true;
            }
        }

        private void KeyUpEventHandler(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.LeftShift || e.Key == Key.RightShift)
            {
                _shiftPressed = false;
            }

            if (e.Key >= Key.A && e.Key <= Key.Z)
            {
                e.Handled = true;
            }
        }
    }
}
