using System.Collections.Concurrent;
using System.Diagnostics;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace ImoutoRebirth.Tori.UI.UserControls;

public partial class RevealImage
{
    private static readonly ConcurrentDictionary<int, SemaphoreSlim> Locker = new();

    public RevealImage() => InitializeComponent();

    public static readonly DependencyProperty SourceProperty =
        DependencyProperty.Register(nameof(Source), typeof(ImageSource), typeof(RevealImage), new(null));

    public ImageSource Source
    {
        get => (ImageSource)GetValue(SourceProperty);
        set => SetValue(SourceProperty, value);
    }

    public static readonly DependencyProperty ShowStepProperty =
        DependencyProperty.Register(nameof(ShowStep), typeof(int), typeof(RevealImage), new(0));

    public int ShowStep
    {
        get => (int)GetValue(ShowStepProperty);
        set => SetValue(ShowStepProperty, value);
    }

    public static readonly DependencyProperty StateProperty =
        DependencyProperty.Register(nameof(State), typeof(int), typeof(RevealImage),
            new(0, OnStateChanged));

    public int State
    {
        get => (int)GetValue(StateProperty);
        set => SetValue(StateProperty, value);
    }

    public static readonly DependencyProperty DelayProperty =
        DependencyProperty.Register(nameof(ShouldDelay), typeof(bool), typeof(RevealImage),
            new(false));

    public bool ShouldDelay
    {
        get => (bool)GetValue(DelayProperty);
        set => SetValue(DelayProperty, value);
    }

    public static readonly DependencyProperty DelayWhenReverseProperty =
        DependencyProperty.Register(nameof(ShouldDelayWhenReverse), typeof(bool), typeof(RevealImage),
            new(false));

    public bool ShouldDelayWhenReverse
    {
        get => (bool)GetValue(DelayWhenReverseProperty);
        set => SetValue(DelayWhenReverseProperty, value);
    }

    private static void OnStateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not RevealImage control) 
            return;

        control.OnStateChangedInternal((int)e.NewValue, (int)e.OldValue);
    }

    private void OnStateChangedInternal(int newState, int oldState)
    {
        var animationDuration = TimeSpan.FromMilliseconds(100);
        var delayDuration = TimeSpan.FromMilliseconds(150);

        if (!IsLoaded)
        {
            Loaded += (_, _) => OnStateChangedInternal(newState, oldState);
            return;
        }

        var width = AnimatedImage.ActualWidth;
        var height = AnimatedImage.ActualHeight;
        
        // reveal animation
        if (newState >= ShowStep && oldState < ShowStep)
        {
            var delayMultiplier = Math.Max(0, ShowStep - oldState);

            var delay = ShouldDelay ? delayDuration : TimeSpan.Zero;
            var newDelay = delay
                           + delayDuration * (delayMultiplier - 1) 
                           + delayDuration * delayMultiplier;

            var animation = new RectAnimation
            {
                From = new Rect(0, 0, 0, height),
                To = new Rect(0, 0, width, height),
                Duration = animationDuration,
                BeginTime = newDelay,
                EasingFunction = new SineEase { EasingMode = EasingMode.EaseOut }
            };
            ClipRect.BeginAnimation(RectangleGeometry.RectProperty, animation);
            IsHitTestVisible = true;
        }

        // hide animation
        else if (newState < ShowStep && oldState >= ShowStep)
        {
            var delayMultiplier = (oldState - ShowStep);
            
            var reverseDelay = ShouldDelayWhenReverse ? delayDuration : TimeSpan.Zero;
            var newDelay = reverseDelay 
                           + delayDuration * (delayMultiplier - 1) 
                           + delayDuration * delayMultiplier;
            
            var animation = new RectAnimation
            {
                From = new Rect(0, 0, width, height),
                To = new Rect(0, 0, 0, height),
                Duration = animationDuration,
                BeginTime = newDelay,
                EasingFunction = new SineEase { EasingMode = EasingMode.EaseIn }
            };
            ClipRect.BeginAnimation(RectangleGeometry.RectProperty, animation);
            IsHitTestVisible = false;
        }
    }
}
