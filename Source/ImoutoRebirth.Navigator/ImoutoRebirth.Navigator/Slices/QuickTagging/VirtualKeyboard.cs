using ControlzEx.Standard;
using System.Runtime.InteropServices;
using System.Windows.Input;

namespace ImoutoRebirth.Navigator.Slices.QuickTagging;

internal static partial class VirtualKeyboard
{
    [LibraryImport("user32.dll")]
    private static partial uint SendInput(uint nInputs, Input[] pInputs, int cbSize);

    [StructLayout(LayoutKind.Sequential)]
    private struct Input
    {
        public uint type;
        public InputUnion u;
    }

    [StructLayout(LayoutKind.Explicit)]
    private struct InputUnion
    {
        [FieldOffset(0)] public Mouseinput mi;
        [FieldOffset(0)] public Keybdinput ki;
        [FieldOffset(0)] public Hardwareinput hi;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct Mouseinput
    {
        public int dx;
        public int dy;
        public uint mouseData;
        public uint dwFlags;
        public uint time;
        public IntPtr dwExtraInfo;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct Keybdinput
    {
        public ushort wVk;
        public ushort wScan;
        public uint dwFlags;
        public uint time;
        public IntPtr dwExtraInfo;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct Hardwareinput
    {
        public uint uMsg;
        public ushort wParamL;
        public ushort wParamH;
    }

    private const uint InputKeyboard = 1;
    private const uint KeyEventFKeyDown = 0;
    private const uint KeyEventFKeyUp = 0x0002;

    //public static void PressAndRelease(Key key)
    //{
    //    var virtualKey = (ushort)KeyInterop.VirtualKeyFromKey(key);

    //    var inputs = new Input[2];

    //    inputs[0].type = InputKeyboard;
    //    inputs[0].u.ki.wVk = virtualKey;
    //    inputs[0].u.ki.dwFlags = KeyEventFKeyDown;

    //    inputs[1].type = InputKeyboard;
    //    inputs[1].u.ki.wVk = virtualKey;
    //    inputs[1].u.ki.dwFlags = KeyEventFKeyUp;

    //    SendInput((uint)inputs.Length, inputs, Marshal.SizeOf(typeof(Input)));
    //}

    public static void PressAndRelease(Key key, params Key[] modifiers)
    {
        var virtualKey = (ushort)KeyInterop.VirtualKeyFromKey(key);


        var inputs = new List<Input>();

        foreach (var modifier in modifiers)
        {
            Input input = default;
            input.type = InputKeyboard;
            input.u.ki.wVk = (ushort)KeyInterop.VirtualKeyFromKey(modifier);
            input.u.ki.dwFlags = KeyEventFKeyDown;
            inputs.Add(input);
        }

        Input keyPressInput = default;
        keyPressInput.type = InputKeyboard;
        keyPressInput.u.ki.wVk = virtualKey;
        keyPressInput.u.ki.dwFlags = KeyEventFKeyDown;
        inputs.Add(keyPressInput);

        Input keyReleaseInput = default;
        keyReleaseInput.type = InputKeyboard;
        keyReleaseInput.u.ki.wVk = virtualKey;
        keyReleaseInput.u.ki.dwFlags = KeyEventFKeyUp;
        inputs.Add(keyReleaseInput);

        foreach (var modifier in modifiers)
        {
            Input input = default;
            input.type = InputKeyboard;
            input.u.ki.wVk = (ushort)KeyInterop.VirtualKeyFromKey(modifier);
            input.u.ki.dwFlags = KeyEventFKeyUp;
            inputs.Add(input);
        }

        var inputsArray = inputs.ToArray();

        SendInput((uint)inputsArray.Length, inputsArray, Marshal.SizeOf(typeof(Input)));
    }

    public static void Press(Key key)
    {
        var virtualKey = (ushort)KeyInterop.VirtualKeyFromKey(key);

        var inputs = new Input[1];

        inputs[0].type = InputKeyboard;
        inputs[0].u.ki.wVk = virtualKey;
        inputs[0].u.ki.dwFlags = KeyEventFKeyDown;

        SendInput((uint)inputs.Length, inputs, Marshal.SizeOf(typeof(Input)));
    }

    public static void Release(Key key)
    {
        var virtualKey = (ushort)KeyInterop.VirtualKeyFromKey(key);

        var inputs = new Input[1];

        inputs[1].type = InputKeyboard;
        inputs[1].u.ki.wVk = virtualKey;
        inputs[1].u.ki.dwFlags = KeyEventFKeyUp;

        SendInput((uint)inputs.Length, inputs, Marshal.SizeOf(typeof(Input)));
    }

    public static Key[] GetPressedModifiers()
    {
        var pressedModifiers = new List<Key>();

        if ((Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
        {
            pressedModifiers.Add(Key.LeftCtrl);
        }

        if ((Keyboard.Modifiers & ModifierKeys.Alt) == ModifierKeys.Alt)
        {
            pressedModifiers.Add(Key.LeftAlt);
        }

        if ((Keyboard.Modifiers & ModifierKeys.Shift) == ModifierKeys.Shift)
        {
            pressedModifiers.Add(Key.LeftShift);
        }

        if ((Keyboard.Modifiers & ModifierKeys.Windows) == ModifierKeys.Windows)
        {
            pressedModifiers.Add(Key.LWin);
        }

        return pressedModifiers.ToArray();
    }
}
