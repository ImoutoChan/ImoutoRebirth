﻿using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ImoutoRebirth.Navigator.UserControls;

/// <summary>
///     Interaction logic for FavoriteControl.xaml
/// </summary>
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
            new PropertyMetadata(false, OnIsCheckedChanged));

    public bool IsChecked
    {
        get => (bool) GetValue(IsCheckedProperty);
        set => SetValue(IsCheckedProperty, value);
    }
    private static void OnIsCheckedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var isChecked = (bool) e.NewValue;
        var control = (FavoriteControl) d;

        control.SetImage(isChecked);
    }

    private void imgRateMinus_MouseEnter(object sender, MouseEventArgs e)
    {
        SetImage(!IsChecked);
    }

    private void imgRatePlus_MouseEnter(object sender, MouseEventArgs e)
    {
        SetImage(!IsChecked);
    }

    private void imgRatePlus_MouseLeave(object sender, MouseEventArgs e)
    {
        SetImage(IsChecked);
    }

    private void gdRating_MouseLeave(object sender, MouseEventArgs e)
    {
        SetImage(IsChecked);
    }

    private void SetImage(bool isFilled)
    {
        var plusImages = pnlPlus.Children.OfType<Border>().ToList();
        for (int i = 0; i < plusImages.Count(); i++)
        {
            var image = plusImages[i];

            image.Visibility = isFilled
                ? Visibility.Visible
                : Visibility.Hidden;
        }
    }

    private void imgRatePlus_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
        IsChecked = !IsChecked;
    }
}