using System.Windows.Controls;
using System.Windows.Input;

namespace ImoutoRebirth.Navigator.UserControls;

public class ExtScrollViewer : ScrollViewer
{
    private void Scrolling(int delta)
    {
        if (ScrollInfo == null)
            return;

        if (delta < 0)
        {
            ScrollInfo.LineDown();
        }
        else
        {
            ScrollInfo.LineUp();
        }
    }

    protected override void OnMouseWheel(MouseWheelEventArgs e)
    {
        if (!(Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl)) && !e.Handled)
        {
            Scrolling(e.Delta);
            e.Handled = true;
            return;
        }

        e.Handled = false;
    }
}
