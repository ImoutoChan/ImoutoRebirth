using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ImoutoRebirth.Tori.UI.UserControls;

/// <summary>
/// Interaction logic for UserControl1.xaml
/// </summary>
public partial class RevealImage : UserControl
{
    public RevealImage()
    {
        InitializeComponent();
    }

    private int _previousState = -1;

    public static readonly DependencyProperty SourceProperty =
        DependencyProperty.Register(nameof(Source), typeof(ImageSource), typeof(RevealImage), new PropertyMetadata(null));

    public ImageSource Source
    {
        get => (ImageSource)GetValue(SourceProperty);
        set => SetValue(SourceProperty, value);
    }

    public static readonly DependencyProperty ShowStepProperty =
        DependencyProperty.Register(nameof(ShowStep), typeof(int), typeof(RevealImage), new PropertyMetadata(0));

    public int ShowStep
    {
        get => (int)GetValue(ShowStepProperty);
        set => SetValue(ShowStepProperty, value);
    }

    public static readonly DependencyProperty StateProperty =
        DependencyProperty.Register(nameof(State), typeof(int), typeof(RevealImage),
            new PropertyMetadata(0, OnStateChanged));

    public int State
    {
        get => (int)GetValue(StateProperty);
        set => SetValue(StateProperty, value);
    }

    public static readonly DependencyProperty DelayProperty =
        DependencyProperty.Register(nameof(Delay), typeof(TimeSpan), typeof(RevealImage),
            new PropertyMetadata(TimeSpan.Zero));

    public TimeSpan Delay
    {
        get => (TimeSpan)GetValue(DelayProperty);
        set => SetValue(DelayProperty, value);
    }

    private static void OnStateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is RevealImage control)
        {
            control.OnStateChangedInternal((int)e.NewValue, (int)e.OldValue);
        }
    }

    private void OnStateChangedInternal(int newState, int oldState)
    {
        if (newState >= ShowStep && oldState < ShowStep)
        {
            AnimateReveal();
        }
        else if (newState < ShowStep)
        {
            // Сброс, если шаг "откатывается"
            ClipRect.Rect = new Rect(0, 0, 0, AnimatedImage.ActualHeight);
        }

        _previousState = newState;
    }

    private void AnimateReveal()
    {
        if (!IsLoaded)
        {
            Loaded += (_, _) => AnimateReveal();
            return;
        }

        double width = AnimatedImage.ActualWidth;
        double height = AnimatedImage.ActualHeight;

        ClipRect.Rect = new Rect(0, 0, 0, height);

        var animation = new RectAnimation
        {
            From = new Rect(0, 0, 0, height),
            To = new Rect(0, 0, width, height),
            Duration = TimeSpan.FromMilliseconds(400),
            BeginTime = Delay,
            EasingFunction = new SineEase { EasingMode = EasingMode.EaseOut }
        };

        ClipRect.BeginAnimation(RectangleGeometry.RectProperty, animation);
    }
}
