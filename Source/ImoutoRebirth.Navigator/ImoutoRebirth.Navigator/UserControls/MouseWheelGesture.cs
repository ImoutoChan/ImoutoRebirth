using System.Windows.Input;

namespace ImoutoRebirth.Navigator.UserControls;

public class MouseWheelGesture : MouseGesture
{
    private WheelDirection Direction { get; set; }

    public static MouseWheelGesture Up => new() { Direction = WheelDirection.Up };

    public static MouseWheelGesture Down => new() { Direction = WheelDirection.Down };

    public static MouseWheelGesture CtrlUp => new(ModifierKeys.Control) { Direction = WheelDirection.Up };

    public static MouseWheelGesture CtrlDown => new(ModifierKeys.Control) { Direction = WheelDirection.Down };

    public static MouseWheelGesture AltUp => new(ModifierKeys.Alt) { Direction = WheelDirection.Up };

    public static MouseWheelGesture AltDown => new(ModifierKeys.Alt) { Direction = WheelDirection.Down };

    public static MouseWheelGesture ShiftUp => new(ModifierKeys.Shift) { Direction = WheelDirection.Up };

    public static MouseWheelGesture ShiftDown => new(ModifierKeys.Shift) { Direction = WheelDirection.Down };

    public MouseWheelGesture() : base(MouseAction.WheelClick)
    { }

    public MouseWheelGesture(ModifierKeys modifiers) : base(MouseAction.WheelClick, modifiers)
    { }

    public override bool Matches(object targetElement, InputEventArgs inputEventArgs)
    {
        if (!base.Matches(targetElement, inputEventArgs)
            || !(inputEventArgs is MouseWheelEventArgs)) 
            return false;

        var args = (MouseWheelEventArgs)inputEventArgs;

        switch (Direction)
        {
            case WheelDirection.None:
                return args.Delta == 0;
            case WheelDirection.Up:
                return args.Delta > 0;
            case WheelDirection.Down:
                return args.Delta < 0;
            default:
                return false;
        }
    }

    private enum WheelDirection
    {
        None,
        Up,
        Down,
    }
}
