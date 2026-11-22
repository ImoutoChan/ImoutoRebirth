using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace ImoutoRebirth.Navigator.UserControls;

public partial class PlayerControl
{
    private Thumb? _seekThumb;
    private bool _resumeAfterSeek;
    private bool _updatingSliderFromPlayer;

    private void OnSliderPreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (Slider.Template?.FindName("PART_Track", Slider) is not Track { Thumb: { } thumb } track || thumb.IsMouseOver)
            return;

        var position = track.ValueFromPoint(e.GetPosition(track));
        if (!double.IsFinite(position))
            return;

        Slider.Value = Math.Clamp(position, Slider.Minimum, Slider.Maximum);
        Slider.UpdateLayout();

        var thumbMouseDown = new MouseButtonEventArgs(e.MouseDevice, e.Timestamp, e.ChangedButton)
        {
            RoutedEvent = MouseLeftButtonDownEvent,
            Source = thumb
        };

        thumb.RaiseEvent(thumbMouseDown);
        e.Handled = true;
    }

    private void OnSeekStarted(object sender, DragStartedEventArgs e)
    {
        _seekThumb = (Thumb)e.OriginalSource;
        _resumeAfterSeek = _isPlaying;

        if (_resumeAfterSeek)
            SetPlaying(false);
    }

    private void OnSeekCompleted(object sender, DragCompletedEventArgs e)
    {
        if (_seekThumb is null)
            return;

        _seekThumb = null;

        if (!e.Canceled)
            SeekTo(Slider.Value, commitPosition: true);

        if (_resumeAfterSeek)
            SetPlaying(true);
    }

    private void ResetPositionSlider()
    {
        var thumb = _seekThumb;
        _seekThumb = null;
        thumb?.CancelDrag();

        SetSliderFromPlayer(0);
    }

    private void SetSliderFromPlayer(double value)
    {
        if (_seekThumb is not null)
            return;

        _updatingSliderFromPlayer = true;

        try
        {
            Slider.Value = value;
        }
        finally
        {
            _updatingSliderFromPlayer = false;
        }
    }

    private void OnSliderValueChanged(object sender, RoutedPropertyChangedEventArgs<double> args)
    {
        if (!_updatingSliderFromPlayer)
            SeekTo(args.NewValue, commitPosition: _seekThumb is null);
    }

    private void SeekTo(double sliderValue, bool commitPosition)
    {
        var position = (float)sliderValue / 100;

        QueueCurrentMediaOperation(player =>
        {
            var duration = player.Length;
            if (duration <= 0)
                return;

            var differenceInMilliseconds = Math.Abs(player.Position - position) * duration;

            if (commitPosition || differenceInMilliseconds >= 1000)
                player.Position = position;
        });
    }
}
