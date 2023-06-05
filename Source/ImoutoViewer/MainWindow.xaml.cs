using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using ImoutoViewer.ViewModel;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;

namespace ImoutoViewer;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
partial class MainWindow
{
    public static MetroWindow CurrentWindow { get; private set; }

    #region Fields

    private bool _isFullscreen;
    private WindowState _lastState;

    #endregion  Fields

    #region Constructors

    public MainWindow()
    {
        InitializeComponent();

        RenderOptions.SetBitmapScalingMode(ViewPort, BitmapScalingMode.Fant);

        CurrentWindow = this;
    }

    #endregion Constructors

    #region Event handlers

    //Fullscreen on f11, alt+enter doesn't work
    protected override void OnPreviewKeyDown(KeyEventArgs e)
    {
        if (e.Key == Key.F11)
        {
            ToggleFullscreen();
            e.Handled = true;
        }

        base.OnPreviewKeyDown(e);
    }

    private void ScrollViewerObject_ScrollChanged(object sender, ScrollChangedEventArgs e)
    {
        //Enable or disable scrolling by mouse wheel (without any modify key)
        var dataContext = DataContext as MainWindowVM;
        if (dataContext != null)
        {
            dataContext.IsSimpleWheelNavigationEnable = (ScrollViewerObject.ComputedVerticalScrollBarVisibility != Visibility.Visible);
        }
    }

    //Open setting flyout
    private void Button_Click(object sender, RoutedEventArgs e)
    {
        CloseAllFlyouts();
        SettingFlyout.IsOpen = !SettingFlyout.IsOpen;
    }

    //Open OpenWith flyout
    private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
    {
        CloseAllFlyouts();
        EditWithFlyout.IsOpen = !EditWithFlyout.IsOpen;
    }

    //Close all flyouts
    private void Client_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
        foreach (Flyout item in Flyouts.Items)
        {
            if (item.IsOpen)
            {
                e.Handled = true;
            }
            item.IsOpen = false;
        }
        Client.Focus();
    }

    //Disable click commands
    private void Disable_MouseButton(object sender, MouseButtonEventArgs e)
    {
        e.Handled = true;
    }

    //Full screen
    private void MainWindow_OnKeyUp(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.S)
        {
            Button_Click(null, null);
        }
        else if (e.Key == Key.Escape)
        {
            (DataContext as MainWindowVM).IsSlideShowActive = false;
            DeactivateFullscreen();
        }
    }

    //Open OpenWith flyout
    private void Client_OnMouseUp(object sender, MouseButtonEventArgs e)
    {
        if (e.ChangedButton == MouseButton.Middle && e.ButtonState == MouseButtonState.Released)
        {
            ButtonBase_OnClick(null, null);
        }
    }

    //Toggle Slideshow
    private void SlideShowButton_OnClick(object sender, RoutedEventArgs e)
    {
        (DataContext as MainWindowVM).ToggleSlideshowCommand.Execute(null);
    }

    //Show add dialog
    private void ShowAddDialog(object sender, RoutedEventArgs e)
    {
        CloseAllFlyouts();
        AddTagFlyout.IsOpen = !AddTagFlyout.IsOpen;
    }

    #endregion Event handlers
    
    #region Methods

    private void DeactivateFullscreen()
    {
        WindowState = WindowState.Normal;
        UseNoneWindowStyle = false;
        Topmost = false;
        ShowTitleBar = true;
        WindowState = _lastState;

        ShowMinButton = true;
        ShowMaxRestoreButton = true;
        ShowCloseButton = true;

        _isFullscreen = false;
    }

    private void ActivateFullscreen()
    {
        _lastState = WindowState;

        WindowState = WindowState.Normal;
        UseNoneWindowStyle = true;
        Topmost = true;
        ShowTitleBar = false;

        ShowMinButton = false;
        ShowMaxRestoreButton = false;
        ShowCloseButton = false;

        WindowState = WindowState.Maximized;

        _isFullscreen = true;
    }

    private void ToggleFullscreen()
    {
        if (_isFullscreen)
        {
            DeactivateFullscreen();
        }
        else
        {
            ActivateFullscreen();
        }
    }

    public void CloseAllFlyouts()
    {
        foreach (Flyout item in Flyouts.Items)
        {
            item.IsOpen = false;
        }
    }

    public async void ShowMessageDialog(string title, string message)
    {
        MetroDialogOptions.ColorScheme = MetroDialogColorScheme.Accented;

        var mySettings = new MetroDialogSettings()
        {
            AffirmativeButtonText = "Ok",
            ColorScheme = MetroDialogColorScheme.Accented
        };

        await this.ShowMessageAsync(title, message, MessageDialogStyle.Affirmative, mySettings);
    }

    public void ShowCreateTagFlyout()
    {
        CloseAllFlyouts();
        CreateTagFlyout.IsOpen = !CreateTagFlyout.IsOpen;
    }
    #endregion Methods
}