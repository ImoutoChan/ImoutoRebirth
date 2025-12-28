using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using MahApps.Metro.Controls;

namespace ImoutoRebirth.Navigator.Slices.FileInfo;

public partial class FileInfoNiceView : UserControl
{
    private bool _lockHovered = false;

    public FileInfoNiceView() => InitializeComponent();

    private FavoriteControl? FavoriteControl => field ??= CardTilt.FindChild<FavoriteControl>();

    private void CardTilt_MouseEnter(object sender, MouseEventArgs e)
    {
        if (_lockHovered)
            return;

        FavoriteControl?.IsHovered = true;
    }

    private void CardTilt_MouseLeave(object sender, MouseEventArgs e)
    {
        _lockHovered = false;
        FavoriteControl?.IsHovered = false;
    }

    private void CardTilt_CardClick(object sender, RoutedEventArgs e)
    {
        _lockHovered = true;
        FavoriteControl?.IsHovered = false;

        FavoriteControl?.Toggle();
    }

    private void CardTilt_CardDoubleClick(object sender, RoutedEventArgs e)
    {
        _lockHovered = true;
        FavoriteControl?.IsHovered = false;

        FavoriteControl?.IsChecked = true;
        RatingControl.Value = 5;
    }
}
