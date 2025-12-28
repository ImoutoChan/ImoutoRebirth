using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace ImoutoRebirth.Navigator.Slices.FileInfo;

public partial class FavoriteControl : UserControl
{
    public FavoriteControl()
    {
        InitializeComponent();
    }

    public static readonly DependencyProperty IsCheckedProperty
        = DependencyProperty.Register(
            nameof(IsChecked),
            typeof(bool),
            typeof(FavoriteControl),
            new(false, OnIsCheckedChanged));

    public static readonly DependencyProperty IsHoveredProperty
        = DependencyProperty.Register(
            nameof(IsHovered),
            typeof(bool),
            typeof(FavoriteControl),
            new(false, OnIsHoveredChanged));

    public bool IsChecked
    {
        get => (bool) GetValue(IsCheckedProperty);
        set => SetValue(IsCheckedProperty, value);
    }

    public bool IsHovered
    {
        get => (bool) GetValue(IsHoveredProperty);
        set => SetValue(IsHoveredProperty, value);
    }

    private static void OnIsCheckedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var control = (FavoriteControl) d;
        control.UpdateVisual();
    }

    private static void OnIsHoveredChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var control = (FavoriteControl) d;
        control.UpdateVisual();
    }

    private void UpdateVisual()
    {
        var showFilled = IsHovered ? !IsChecked : IsChecked;

        FavItem.Source = (DrawingImage)FindResource(
            showFilled
                ? "HeartFilledDrawingImage"
                : "HeartOutlineDrawingImage");

        FavItemBorder.ToolTip = IsChecked
            ? "Remove from favorites"
            : "Add to favorites";
    }

    public void Toggle() => IsChecked = !IsChecked;
}