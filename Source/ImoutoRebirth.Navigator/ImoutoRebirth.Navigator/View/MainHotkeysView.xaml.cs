using System.Windows.Controls;

namespace ImoutoRebirth.Navigator.View;

public partial class MainHotkeysView : UserControl
{
    public MainHotkeysView()
    {
        InitializeComponent();
        DataContext = new MainHotkeysViewModel();
    }
}

public class MainHotkeysViewModel
{
    public List<HotkeyItem> Hotkeys { get; } =
    [
        new("Double Left 🖱️", "open file"),
        new("Right 🖱️", "context menu"),
        new("CTRL + 🖱️ Wheel", "zoom previews"),
        new("CTRL + C", "selected to clipboard"),
        new("Middle 🖱️", "fullscreen preview"),
        new("W A S D", "like arrow keys"),
        new("T", "add tags"),
        new("M", "merge tags"),
        new("F5", "refresh"),
        new("CTRL + Q", "quick tagging"),
    ];
}

public record HotkeyItem(string Key, string Description);
