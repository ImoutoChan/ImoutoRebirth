using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using MahApps.Metro.IconPacks;

namespace ImoutoRebirth.Navigator.Slices.FileInfo;

public partial class RatingControl
{
    private int _ratingUnderMouse;

    public RatingControl() => InitializeComponent();

    public static readonly DependencyProperty ValueProperty
        = DependencyProperty.Register(
            nameof(Value), 
            typeof(int), 
            typeof(RatingControl),
            new(0, OnValueChanged));

    public int Value
    {
        get
        {
            try
            {
                return (int) GetValue(ValueProperty);
            }
            catch
            {
                return 0;
            }
        }
        set => SetValue(ValueProperty, value);
    }

    private static void OnValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var rating = (int) e.NewValue;
        var control = (RatingControl) d;

        control.RenderStarredValue(rating);
    }

    private void StarBorder_MouseEnter(object sender, MouseEventArgs e) => RenderStarredValueUnderMouse(sender);

    private void StarBorder_MouseLeave(object sender, MouseEventArgs e) => RenderStarredValueUnderMouse(sender);

    private void OuterGridRating_MouseLeave(object sender, MouseEventArgs e) => RenderStarredValue(Value);

    private void RenderStarredValueUnderMouse(object sender)
    {
        if (sender is not Border border)
            return;

        SetRatingUnderMouse(border);
        RenderStarredValue(_ratingUnderMouse);
    }

    private void RenderStarredValue(int value)
    {
        var minusImages = RatingPanel.Children.OfType<Border>().ToList();

        for (int i = 0; i < minusImages.Count(); i++)
        {
            var image = minusImages[i];
            var star = (PackIconMaterial)image.Child;

            var starHighlighted = i + 1 <= value;
            var lastSelectedStart = i + 1 == value;

            star.Kind = starHighlighted 
                ? PackIconMaterialKind.Star 
                : PackIconMaterialKind.StarOutline;

            if (lastSelectedStart)
            {
                star.Width = 41;
                star.Height = 41;
                star.Margin = new(-1, -4, -1, 0);
            }
            else
            {
                star.Width = 33;
                star.Height = 33;
                star.Margin = new(3, 0, 3, 0);
            }
        }
    }

    private void StarBorder_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
        if (sender is not Border img)
            return;

        SetRatingUnderMouse(img);
        Value = _ratingUnderMouse;
    }

    private void SetRatingUnderMouse(Border img)
    {
        var rating = ((Panel)img.Parent).Children.IndexOf(img) + 1;
        _ratingUnderMouse = rating;
    }
}
