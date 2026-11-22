using System.Collections.Concurrent;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using ImoutoRebirth.Common.WPF;
using Serilog;
using Vlc.DotNet.Core;
using Vlc.DotNet.Wpf;
using Application = System.Windows.Application;

namespace ImoutoRebirth.Navigator.UserControls;

public partial class PlayerControl
{
    private static readonly ConcurrentDictionary<string, long> LastMediaPositionValues = new();

    private VlcControl? _control;
    private Task _playerOperations = Task.CompletedTask;
    private Action? _detachPlaybackEvents;
    private int _mediaVersion;
    private bool _isPlaying;

    public PlayerControl()
    {
        InitializeComponent();

        Loaded += (_, _) => ChangeSource(Source);
        Unloaded += (_, _) => ReleasePlayer();

        Slider.AddHandler(Thumb.DragStartedEvent, new DragStartedEventHandler(OnSeekStarted));
        Slider.AddHandler(Thumb.DragCompletedEvent, new DragCompletedEventHandler(OnSeekCompleted));
        Slider.PreviewMouseLeftButtonDown += OnSliderPreviewMouseLeftButtonDown;
        Slider.ValueChanged += OnSliderValueChanged;
    }

    public static void ResetLastMediaPositions() => LastMediaPositionValues.Clear();

    private static VlcMediaPlayer? GetOrCreatePlayer(VlcControl control)
    {
        var provider = control.SourceProvider;

        if (provider.MediaPlayer is { } player)
            return player;

        if (!TryGetVlcLibDirectory(out var vlcLibDirectory))
            return null;

        Log.Information("Using VLC library at {Path}", vlcLibDirectory?.FullName);

        provider.CreatePlayer(vlcLibDirectory);
        return provider.MediaPlayer;
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
        var control = (PlayerControl)d;

        if (control.IsLoaded)
            control.ChangeSource((string?)e.NewValue);
    }

    private void ChangeSource(string? source)
    {
        var version = Interlocked.Increment(ref _mediaVersion);
        ShowPlaying(false);
        ResetPositionSlider();
        TimeTextBlock.Text = string.Empty;

        if (source is null)
        {
            ClearMedia();
            return;
        }

        if (_control is null)
        {
            _control = new VlcControl();
            Container.Content = _control;
        }

        var control = _control;
        var volume = Volume;
        ShowPlaying(true);

        QueuePlayerOperation(() =>
        {
            if (version != Volatile.Read(ref _mediaVersion))
                return;

            if (GetOrCreatePlayer(control) is not { } player || version != Volatile.Read(ref _mediaVersion))
                return;

            DetachPlaybackEvents();
            player.Stop();

            var options = LastMediaPositionValues.TryGetValue(source, out var position)
                ? new[] { "input-repeat=65535", $"start-time={position / 1000}" }
                : new[] { "input-repeat=65535" };

            player.SetMedia(new FileInfo(source), options);
            AttachPlaybackEvents(player, source, version);
            player.Audio.Volume = volume;
            player.Play();
        });
    }

    private void ClearMedia()
    {
        if (!InfinityLifespanMode)
        {
            ReleasePlayer();
            return;
        }

        if (_control is { } control)
        {
            QueuePlayerOperation(() =>
            {
                DetachPlaybackEvents();

                if (control.SourceProvider.MediaPlayer is { } player)
                {
                    player.Stop();
                    player.ResetMedia();
                }
            });
        }
    }

    private void ReleasePlayer()
    {
        if (_control is not { } control)
            return;

        Interlocked.Increment(ref _mediaVersion);
        _control = null;
        Container.Content = null;

        QueuePlayerOperation(() =>
        {
            DetachPlaybackEvents();
            control.SourceProvider.MediaPlayer?.Stop();
            control.Dispose();
        });
    }

    private void AttachPlaybackEvents(
        VlcMediaPlayer player,
        string source,
        int version)
    {
        long length = 0;

        void OnLengthChanged(object? sender, VlcMediaPlayerLengthChangedEventArgs args)
            => Interlocked.Exchange(ref length, args.NewLength);

        void OnPositionChanged(object? sender, VlcMediaPlayerPositionChangedEventArgs args)
            => Dispatcher.InvokeAsync(() =>
            {
                if (version == _mediaVersion)
                    SetSliderFromPlayer(100 * args.NewPosition);
            });

        void OnTimeChanged(object? sender, VlcMediaPlayerTimeChangedEventArgs args)
        {
            var time = args.NewTime;
            var duration = Interlocked.Read(ref length);

            if (duration > 0 && time > duration * 0.9)
            {
                LastMediaPositionValues.TryRemove(source, out _);
            }
            else
            {
                LastMediaPositionValues[source] = time;
            }

            Dispatcher.InvokeAsync(() =>
            {
                if (version == _mediaVersion)
                    TimeTextBlock.Text = $"{GetPrettyTime(time, duration)} / {GetPrettyTime(duration)}";
            });
        }

        player.LengthChanged += OnLengthChanged;
        player.PositionChanged += OnPositionChanged;
        player.TimeChanged += OnTimeChanged;

        _detachPlaybackEvents = () =>
        {
            player.LengthChanged -= OnLengthChanged;
            player.PositionChanged -= OnPositionChanged;
            player.TimeChanged -= OnTimeChanged;
        };
    }

    private void DetachPlaybackEvents()
    {
        _detachPlaybackEvents?.Invoke();
        _detachPlaybackEvents = null;
    }

    private void QueuePlayerOperation(Action operation)
    {
        _playerOperations = _playerOperations.ContinueWith(
            _ =>
            {
                try
                {
                    operation();
                }
                catch (Exception e)
                {
                    Log.Warning(e, "Navigator video operation failed");
                }
            },
            CancellationToken.None,
            TaskContinuationOptions.None,
            TaskScheduler.Default);
    }

    private void QueueCurrentMediaOperation(Action<VlcMediaPlayer> operation)
    {
        if (Source is null || _control is not { } control)
            return;

        var version = _mediaVersion;

        QueuePlayerOperation(() =>
        {
            if (version == Volatile.Read(ref _mediaVersion) && control.SourceProvider.MediaPlayer is { } player)
                operation(player);
        });
    }

    private void ShowPlaying(bool isPlaying)
    {
        _isPlaying = isPlaying;
        var icon = isPlaying ? "VideoPauseOverlayIcon" : "VideoPlayOverlayIcon";
        PlayButton.Template = Application.Current.FindResource(icon) as ControlTemplate;
    }

    private void SetPlaying(bool isPlaying)
    {
        if (Source is null || _control is null)
            return;

        ShowPlaying(isPlaying);

        QueueCurrentMediaOperation(player =>
        {
            if (isPlaying)
            {
                player.Play();
            }
            else
            {
                player.SetPause(true);
            }
        });
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
            
        if (shouldPause && control._isPlaying)
            control.SetPlaying(false);
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

        control.QueueCurrentMediaOperation(player => player.Audio.Volume = volume);
    }

    private void PlayButton_OnClick(object sender, RoutedEventArgs e)
    {
        SetPlaying(!_isPlaying);
    }

    private void Root_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (e.OriginalSource is DependencyObject d)
        {
            var parentButton = d.GetParentOfType<Button>();
            var parentSlider = d.GetParentOfType<Slider>();

            if (parentButton != null || parentSlider != null)
                return;
        }

        SetPlaying(!_isPlaying);
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
