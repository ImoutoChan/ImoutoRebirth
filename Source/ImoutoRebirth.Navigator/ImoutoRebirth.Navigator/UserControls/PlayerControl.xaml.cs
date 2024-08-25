using System.Collections.Concurrent;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using Vlc.DotNet.Wpf;

namespace ImoutoRebirth.Navigator.UserControls;

/// <summary>
///     Interaction logic for PlayerControl.xaml
/// </summary>
public partial class PlayerControl
{
    private static readonly ConcurrentDictionary<string, long> LastMediaPositionValues = new();
    
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

    [MemberNotNull(nameof(_control))]
    private void InitializePlayer()
    {
        _control = new VlcControl();
        Container.Content = _control;

        if (!TryGetVlcLibDirectory(out var vlcLibDirectory)) 
            return;
        
        _control.SourceProvider.CreatePlayer(vlcLibDirectory);
        _control.SourceProvider.MediaPlayer.PositionChanged 
            += (_, args) 
                =>
            {
                try
                {
                    Dispatcher.BeginInvoke((Action)(() => { Slider.Value = 100 * args.NewPosition; }));
                }
                catch (Exception e)
                {
                    Debug.WriteLine("Ignoring player error: " + e);
                }
            };

        _control.SourceProvider.MediaPlayer.TimeChanged
            += (_, args)
                =>
            {
                try
                {
                    var time = args.NewTime;
                    var length = _control.SourceProvider.MediaPlayer.Length;
                    var timeString = $"{GetPrettyTime(time, length)} / {GetPrettyTime(length)}";
                    Dispatcher.BeginInvoke((Action)(() =>
                    {
                        var source = Source;
                        if (source == null) 
                            return;
                        
                        LastMediaPositionValues[source] = time;
                        TimeTextBlock.Text = timeString;
                    }));
                }
                catch (Exception e)
                {
                    Debug.WriteLine("Ignoring player error: " + e);
                }
            };

        Slider.ValueChanged += (_, args) =>
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

    private static bool TryGetVlcLibDirectory(out DirectoryInfo? vlcLibDirectory)
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
            nameof(Source), 
            typeof(string), 
            typeof(PlayerControl), 
            new UIPropertyMetadata(null, OnSourceChanged));

    public string? Source
    {
        get => (string?) GetValue(SourceProperty);
        set => SetValue(SourceProperty, value);
    }

    public static readonly DependencyProperty InfinityLifespanModeProperty 
        = DependencyProperty.Register(
            nameof(InfinityLifespanMode), 
            typeof (bool), 
            typeof (PlayerControl), 
            new UIPropertyMetadata(false, null));

    public bool InfinityLifespanMode
    {
        get => (bool) GetValue(InfinityLifespanModeProperty);
        set => SetValue(InfinityLifespanModeProperty, value);
    }

    private static void OnSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var newPropertyValue = (string?) e.NewValue;
        var control = (PlayerControl) d;
        var player = control._control.SourceProvider.MediaPlayer;

        control.Dispatcher.BeginInvoke((Action)(() =>
        {
            if (newPropertyValue == null)
            {
                if (!control.InfinityLifespanMode)
                {
                    Task.Run(() => control._control.Dispose());
                }
                else
                {
                    player?.Pause();
                    control.IsPlayed = false;
                }
            }
            else
            {
                var mediaOptions = new[] { "input-repeat=65535" };
                
                if (LastMediaPositionValues.TryGetValue(newPropertyValue, out var position))
                    mediaOptions = mediaOptions.Append("start-time=" + position / 1000).ToArray();
                
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
            nameof(ShouldPause), 
            typeof (bool), 
            typeof (PlayerControl), 
            new UIPropertyMetadata(false, OnShouldPauseChanged));

    private static void OnShouldPauseChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var shouldPause = (bool) e.NewValue;
        var control = (PlayerControl) d;
            
        if (shouldPause && control.IsPlayed)
        {
            control.IsPlayed = false;
        }
    }

    public int Volume
    {
        get => (int) GetValue(VolumeProperty);
        set => SetValue(VolumeProperty, value);
    }

    public static readonly DependencyProperty VolumeProperty 
        = DependencyProperty.Register(
            nameof(Volume), 
            typeof (int), 
            typeof (PlayerControl), 
            new UIPropertyMetadata(0, OnVolumeChanged));

    public bool IsVolumeVisible
    {
        get => (bool) GetValue(IsVolumeVisibleProperty);
        set => SetValue(IsVolumeVisibleProperty, value);
    }

    public static readonly DependencyProperty IsVolumeVisibleProperty 
        = DependencyProperty.Register(
            nameof(IsVolumeVisible), 
            typeof (bool), 
            typeof (PlayerControl), 
            new UIPropertyMetadata(false));

    private static void OnVolumeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var volume = (int) e.NewValue;
        var control = (PlayerControl) d;

        var provider = control._control.SourceProvider;

        if (provider != null)
            provider.MediaPlayer.Audio.Volume = volume;
    }

    private void PlayButton_OnClick(object sender, RoutedEventArgs e)
    {
        IsPlayed = !IsPlayed;
    }

    private static string GetPrettyTime(long time, long? length = null)
    {
        length ??= time;
        
        var timeSpan = TimeSpan.FromMilliseconds(time);
        var lengthTimeSpan = TimeSpan.FromMilliseconds(length.Value);
        var hours = timeSpan.Hours;
        var minutes = timeSpan.Minutes;
        var seconds = timeSpan.Seconds;
        var lengthHours = lengthTimeSpan.Hours;

        var hoursString = lengthHours > 0 ? $"{hours:00}:" : string.Empty;
        var minutesString = $"{minutes:00}:";
        var secondsString = $"{seconds:00}";

        return $"{hoursString}{minutesString}{secondsString}";
    }
}
