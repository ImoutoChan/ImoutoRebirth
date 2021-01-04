using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Vlc.DotNet.Wpf;

namespace ImoutoRebirth.Navigator.UserControls
{
    /// <summary>
    ///     Interaction logic for PlayerControl.xaml
    /// </summary>
    public partial class PlayerControl
    {
        private bool _isPlayed;
        private VlcControl _control;

        private bool IsPlayed
        {
            get => _isPlayed;
            set
            {
                _isPlayed = value;

                switch (_isPlayed)
                {
                    case false:
                        _control.SourceProvider.MediaPlayer?.Pause();
                        PlayButton.Template = Application.Current.FindResource("VideoPlayOverlayIcon") as ControlTemplate;
                        break;
                    case true:
                        _control.SourceProvider.MediaPlayer?.Play();
                        PlayButton.Template = Application.Current.FindResource("VideoPauseOverlayIcon") as ControlTemplate;
                        break;
                }
            }
        }

        public PlayerControl()
        {
            InitializeComponent();

            InitializePlayer();
        }

        private void InitializePlayer()
        {
            if (!TryGetVlcLibDirectory(out var vlcLibDirectory)) 
                return;

            _control = new VlcControl();
            Container.Content = _control;

            _control.SourceProvider.CreatePlayer(vlcLibDirectory);
            _control.SourceProvider.MediaPlayer.PositionChanged 
                += (sender, args) 
                    => Dispatcher.BeginInvoke((Action) (() => { Slider.Value = 100 * args.NewPosition; }));

            Slider.ValueChanged += (sender, args) =>
            {
                var player = _control.SourceProvider.MediaPlayer;

                if (player == null)
                    return;

                var newValue = (float) args.NewValue / 100;
                var oldValue = player.Position;

                var absoluteDiff = Math.Abs(oldValue - newValue);
                var length = player.Length;

                var msDiff = absoluteDiff * length;

                if (msDiff < 1000) 
                    return;

                var set = (float) args.NewValue / 100;

                Task.Run(() => player.Position = set);
            };
        }

        private static bool TryGetVlcLibDirectory(out DirectoryInfo vlcLibDirectory)
        {
            var currentAssembly = Assembly.GetEntryAssembly();
            vlcLibDirectory = null;

            if (currentAssembly == null)
                return false;

            var currentDirectory = new FileInfo(currentAssembly.Location).DirectoryName;
            vlcLibDirectory = new DirectoryInfo(
                Path.Combine(
                    currentDirectory ?? string.Empty,
                    "libvlc",
                    IntPtr.Size == 4 ? "win-x86" : "win-x64"));

            return true;
        }

        public static readonly DependencyProperty SourceProperty 
            = DependencyProperty.Register(
                "Source", 
                typeof (string), 
                typeof (PlayerControl), 
                new UIPropertyMetadata(null, OnSourceChanged));

        public string Source
        {
            get => (string) GetValue(SourceProperty);
            set => SetValue(SourceProperty, value);
        }

        private static void OnSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var newPropertyValue = (string) e.NewValue;
            var control = (PlayerControl) d;
            var player = control._control.SourceProvider.MediaPlayer;

            control.Dispatcher.BeginInvoke((Action)(() =>
            {
                if (newPropertyValue == null)
                {
                    Task.Run(() => control._control.Dispose());
                }
                else
                {
                    var mediaOptions = new[] { "input-repeat=65535" };
                    player.SetMedia(new FileInfo(newPropertyValue), mediaOptions);
                    control.IsPlayed = true;
                }
            }));
        }

        public bool ShouldPause
        {
            get => (bool) GetValue(ShouldPauseProperty);
            set => SetValue(ShouldPauseProperty, value);
        }

        public static readonly DependencyProperty ShouldPauseProperty 
            = DependencyProperty.Register(
                "ShouldPause", 
                typeof (bool), 
                typeof (PlayerControl), 
                new UIPropertyMetadata(false, OnShouldPauseChanged));

        private static void OnShouldPauseChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var shouldPause = (bool) e.NewValue;
            var control = (PlayerControl) d;
            
            if (shouldPause)
            {
                control.IsPlayed = false;
            }
        }

        private void PlayButton_OnClick(object sender, RoutedEventArgs e)
        {
            IsPlayed = !IsPlayed;
        }
    }
}
