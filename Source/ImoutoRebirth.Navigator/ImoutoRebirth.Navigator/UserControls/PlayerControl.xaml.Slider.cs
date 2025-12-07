using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using Serilog;

namespace ImoutoRebirth.Navigator.UserControls;

public partial class PlayerControl
{
    private bool _wasPlayingBeforeSliderTouched = false;
    private Track? _track;
    private bool _dragHandlersAttached;
    private DragStartedEventHandler? _dragStartedHandler;
    private DragCompletedEventHandler? _dragCompletedHandler;

    private void SliderOnUnloaded(object sender, RoutedEventArgs e)
    {
        Slider.ValueChanged -= OnSliderOnValueChanged;

        if (_dragHandlersAttached)
        {
            if (_dragStartedHandler != null)
                Slider.RemoveHandler(Thumb.DragStartedEvent, _dragStartedHandler);

            if (_dragCompletedHandler != null)
                Slider.RemoveHandler(Thumb.DragCompletedEvent, _dragCompletedHandler);

            _dragHandlersAttached = false;
        }

        if (_track != null)
        {
            _track.PreviewMouseLeftButtonDown -= TrackOnPreviewMouseLeftButtonDown;
            _track = null;
        }
    }

    private void SliderOnLoaded(object sender, RoutedEventArgs e)
    {
        Slider.ValueChanged += OnSliderOnValueChanged;

        if (!_dragHandlersAttached)
        {
            _dragStartedHandler ??= HandleDragStartedEvent;
            _dragCompletedHandler ??= HandleDragCompletedEvent;

            Slider.AddHandler(Thumb.DragStartedEvent, _dragStartedHandler);
            Slider.AddHandler(Thumb.DragCompletedEvent, _dragCompletedHandler);
            _dragHandlersAttached = true;
        }

        var track = (Track)Slider.Template.FindName("PART_Track", Slider);
        if (track != null)
        {
            _track = track;
            _track.PreviewMouseLeftButtonDown += TrackOnPreviewMouseLeftButtonDown;
        }
    }

    private void TrackOnPreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        var track = (Track)sender;
        var slider = (Slider)track.TemplatedParent;

        if (track.Thumb is { IsMouseOver: true }) 
            return;

        var currentPosition = e.GetPosition(track);
        double ratio;

        if (track.Orientation == Orientation.Horizontal)
        {
            ratio = currentPosition.X / track.ActualWidth;
        }
        else
        {
            ratio = 1.0 - (currentPosition.Y / track.ActualHeight);
        }

        ratio = Math.Clamp(ratio, 0.0, 1.0);
        slider.Value = slider.Minimum + ratio * (slider.Maximum - slider.Minimum);

        slider.UpdateLayout();

        if (track.Thumb == null) 
            return;

        var args = new MouseButtonEventArgs(e.MouseDevice, e.Timestamp, e.ChangedButton)
        {
            RoutedEvent = UIElement.MouseLeftButtonDownEvent,
            Source = track.Thumb
        };

        track.Thumb.RaiseEvent(args);
        e.Handled = true;
    }

    private void HandleDragCompletedEvent(object s, DragCompletedEventArgs e)
    {
        if (_wasPlayingBeforeSliderTouched)
            IsPlaying = true;
    }

    private void HandleDragStartedEvent(object s, DragStartedEventArgs e)
    {
        if (IsPlaying)
        {
            _wasPlayingBeforeSliderTouched = true;
            IsPlaying = false;
        }
        else
        {
            _wasPlayingBeforeSliderTouched = false;
        }
    }

    private void OnSliderOnValueChanged(object _, RoutedPropertyChangedEventArgs<double> args)
    {
        try
        {
            if (_control.SourceProvider.MediaPlayer == null || _isDisposed)
                return;

            var newValue = (float)args.NewValue / 100;

            if (_isDisposed)
                return;

            var oldValue = _control.SourceProvider.MediaPlayer.Position;

            var absoluteDiff = Math.Abs(oldValue - newValue);

            if (_isDisposed)
                return;

            var length = _control.SourceProvider.MediaPlayer.Length;

            var msDiff = absoluteDiff * length;

            if (msDiff < 1000) return;

            var set = (float)args.NewValue / 100;

            if (_isDisposed)
                return;

            Task.Run(() => _control.SourceProvider.MediaPlayer.Position = set);
        }
        catch (Exception e)
        {
            Log.Warning(e, "Ignoring player error on slider change");
        }
    }
}
