using System;
using System.Windows;
using System.Windows.Controls;
using Meta.Vlc.Wpf;

namespace Imouto.Navigator.UserControls
{
    /// <summary>
    ///     Interaction logic for PlayerControl.xaml
    /// </summary>
    public partial class PlayerControl
    {
        private bool IsPlayed
        {
            get
            {
                return _isPlayed;
            }
            set
            {
                _isPlayed = value;

                switch (_isPlayed)
                {
                    case false:
                        vlcPlayer.PauseOrResume();
                        PlayButton.Template = Application.Current.FindResource("VideoPlayOverlayIcon") as ControlTemplate;
                        break;
                    case true:
                        vlcPlayer.PauseOrResume();
                        PlayButton.Template = Application.Current.FindResource("VideoPauseOverlayIcon") as ControlTemplate;
                        break;
                }
            }
        }

        public PlayerControl()
        {
            InitializeComponent();
        }

        public static readonly DependencyProperty SourceProperty = DependencyProperty.Register("Source", typeof (string), typeof (PlayerControl), new UIPropertyMetadata(null, OnSourceChanged));

        private bool _isPlayed;

        public string Source
        {
            get
            {
                return (string) GetValue(SourceProperty);
            }
            set
            {
                SetValue(SourceProperty, value);
            }
        }

        private static void OnSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var newPropertyValue = (string) e.NewValue;
            var control = ((PlayerControl) d);
            var player = control.vlcPlayer;

            player.Dispatcher.BeginInvoke((Action)(() =>
            {
                if (newPropertyValue == null)
                {
                    control.IsPlayed = false;
                    player.Stop();
                    //player.Dispose();
                }
                else
                {
                    player.RebuildPlayer();
                    player.LoadMedia(newPropertyValue);
                    player.EndBehavior = EndBehavior.Repeat;
                    player.Play();
                    control.IsPlayed = true;
                }
            }));
        }

        private void PlayButton_OnClick(object sender, RoutedEventArgs e)
        {
            IsPlayed = !IsPlayed;
        }
    }
}
