using System.Windows.Controls;
using System.Windows.Input;

namespace ImoutoViewer.View;

/// <summary>
/// Interaction logic for AddTagV.xaml
/// </summary>
partial class CreateTagV : UserControl
{
    private bool _shiftPressed;

    public CreateTagV()
    {
        InitializeComponent();
    }

    private void KeyDownEventHandler(object sender, KeyEventArgs e)
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