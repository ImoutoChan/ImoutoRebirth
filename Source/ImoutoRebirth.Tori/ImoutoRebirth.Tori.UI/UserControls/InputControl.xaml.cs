using System.Windows;
using System.Windows.Controls;

namespace ImoutoRebirth.Tori.UI.UserControls;

public partial class InputControl : UserControl
{
    public InputControl() => InitializeComponent();

    public static readonly DependencyProperty HeaderProperty
        = DependencyProperty.Register(
            nameof(Header),
            typeof(string),
            typeof(InputControl),
            new UIPropertyMetadata(null));

    public string? Header
    {
        get => (string?) GetValue(HeaderProperty);
        set => SetValue(HeaderProperty, value);
    }

    public static readonly DependencyProperty TextProperty
        = DependencyProperty.Register(
            nameof(Text),
            typeof(string),
            typeof(InputControl),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

    public string? Text
    {
        get => (string?) GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    public static readonly DependencyProperty IsRequiredProperty
        = DependencyProperty.Register(
            nameof(IsRequired),
            typeof(bool),
            typeof(InputControl),
            new FrameworkPropertyMetadata(false));

    public bool IsRequired
    {
        get => (bool) GetValue(IsRequiredProperty);
        set => SetValue(IsRequiredProperty, value);
    }
}
