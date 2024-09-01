using System.Collections.Concurrent;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using LibVLCSharp.Shared;
using Serilog;
using Application = System.Windows.Application;

namespace ImoutoRebirth.Navigator.UserControls;

/// <summary>
///     Interaction logic for AltPlayerControl.xaml
/// </summary>
public partial class AltPlayerControl
{
    private static readonly ConcurrentDictionary<string, long> LastMediaPositionValues = new();
    
    private bool _isPlayed;
    private bool _isDisposed = false;
    private LibVLC? _libVlc;
    private MediaPlayer? _mediaPlayer;

    private bool IsPlayed
    {
        get => _isPlayed;
        set
        {
            _isPlayed = value;

            switch (_isPlayed)
            {
                case false:
                    _mediaPlayer?.Pause();
                    PlayButton.Template = Application.Current.FindResource("VideoPlayOverlayIcon") as ControlTemplate;
                    break;
                case true:
                    _mediaPlayer?.Play();
                    PlayButton.Template = Application.Current.FindResource("VideoPauseOverlayIcon") as ControlTemplate;
                    break;
            }
        }
    }

    public AltPlayerControl()
    {
        InitializeComponent();

        VideoView.IsVisibleChanged += VideoView_Loaded;
        VideoView.IsVisibleChanged += VideoView_Unloaded;
    }

    private void VideoView_Unloaded(object sender, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
    {
        if (!VideoView.IsVisible)
        {
            _isDisposed = true;
            _mediaPlayer?.Stop();
            _mediaPlayer?.Dispose();
            _libVlc?.Dispose();
        }
    }

    private void VideoView_Loaded(object sender, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
    {
        if (VideoView.IsVisible)
            InitializePlayer();
    }

    private void InitializePlayer()
    {
        _libVlc = new LibVLC(enableDebugLogs: true);
        VideoView.MediaPlayer = new MediaPlayer(_libVlc);

        //if (!TryGetVlcLibDirectory(out var vlcLibDirectory)) 
        //    return;

        _mediaPlayer = VideoView.MediaPlayer;
        if (Source != null)
        {
            _mediaPlayer.Media = new Media(_libVlc, new Uri(Source));
            IsPlayed = true;
        }

        //_mediaPlayer.PositionChanged 
        //    += (_, args) 
        //        =>
        //    {
        //        try
        //        {
        //            Dispatcher.BeginInvoke((Action)(() => { Slider.Value = 100 * args.Position; }));
        //        }
        //        catch (Exception e)
        //        {
        //            Log.Warning(e, "Ignoring player error on position change");
        //        }
        //    };

        //_mediaPlayer.TimeChanged
        //    += (_, args)
        //        =>
        //    {
        //        try
        //        {
        //            var time = args.Time;
        //            var length = _mediaPlayer.Length;
        //            var timeString = $"{GetPrettyTime(time, length)} / {GetPrettyTime(length)}";
        //            Dispatcher.BeginInvoke((Action)(() =>
        //            {
        //                var source = Source;
        //                if (source == null) 
        //                    return;

        //                LastMediaPositionValues[source] = time;
        //                TimeTextBlock.Text = timeString;
        //            }));
        //        }
        //        catch (Exception e)
        //        {
        //            Log.Warning(e, "Ignoring player error on time change");
        //        }
        //    };

        //Slider.ValueChanged += OnSliderOnValueChanged;
    }
    
    private void OnSliderOnValueChanged(object _, RoutedPropertyChangedEventArgs<double> args)
    {
        try
        {
            if (_mediaPlayer == null || _isDisposed)
                return;

            var newValue = (float)args.NewValue / 100;

            if (_isDisposed)
                return;

            var oldValue = _mediaPlayer.Position;

            var absoluteDiff = Math.Abs(oldValue - newValue);

            if (_isDisposed)
                return;

            var length = _mediaPlayer.Length;

            var msDiff = absoluteDiff * length;

            if (msDiff < 1000) return;

            var set = (float)args.NewValue / 100;

            if (_isDisposed)
                return;

            Task.Run(() => _mediaPlayer.Position = set);
        }
        catch (Exception e)
        {
            Log.Warning(e, "Ignoring player error on slider change");
        }
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
            typeof(AltPlayerControl), 
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
            typeof (AltPlayerControl), 
            new UIPropertyMetadata(false, null));

    public bool InfinityLifespanMode
    {
        get => (bool) GetValue(InfinityLifespanModeProperty);
        set => SetValue(InfinityLifespanModeProperty, value);
    }

    private static void OnSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var newPropertyValue = (string?) e.NewValue;
        var control = (AltPlayerControl) d;
        var player = control._mediaPlayer;

        control.Dispatcher.BeginInvoke((Action)(() =>
        {
            if (newPropertyValue == null)
            {
                if (!control.InfinityLifespanMode)
                {
                    Task.Run(() =>
                    {
                        control._isDisposed = true;
                        // ?
                    });
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
                
                if (player == null || control._libVlc == null)
                    return;
                
                player.Media = new Media(control._libVlc, new Uri(newPropertyValue));
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
            typeof (AltPlayerControl), 
            new UIPropertyMetadata(false, OnShouldPauseChanged));

    private static void OnShouldPauseChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var shouldPause = (bool) e.NewValue;
        var control = (AltPlayerControl) d;
            
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
            typeof (AltPlayerControl), 
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
            typeof (AltPlayerControl), 
            new UIPropertyMetadata(false));

    private static void OnVolumeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var volume = (int) e.NewValue;
        var control = (AltPlayerControl) d;


        if (control._mediaPlayer == null)
            return;

        control._mediaPlayer.Volume = volume;
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
