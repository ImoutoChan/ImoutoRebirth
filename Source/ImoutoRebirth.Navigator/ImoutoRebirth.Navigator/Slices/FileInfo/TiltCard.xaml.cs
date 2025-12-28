using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace ImoutoRebirth.Navigator.Slices.FileInfo;

public partial class TiltCard : UserControl
{
    public static readonly DependencyProperty CardContentProperty =
        DependencyProperty.Register(
            nameof(CardContent),
            typeof(object),
            typeof(TiltCard),
            new(null));

    public static readonly DependencyProperty MaxAngleProperty =
        DependencyProperty.Register(nameof(MaxAngle), typeof(double), typeof(TiltCard), new(10.0));

    public static readonly DependencyProperty HoverScaleProperty =
        DependencyProperty.Register(
            nameof(HoverScale),
            typeof(double),
            typeof(TiltCard),
            new(1.03));

    public static readonly DependencyProperty SmoothProperty =
        DependencyProperty.Register(nameof(Smooth), typeof(double), typeof(TiltCard), new(0.18));

    public static readonly DependencyProperty CardWidthProperty =
        DependencyProperty.Register(
            nameof(CardWidth),
            typeof(double),
            typeof(TiltCard),
            new(360.0));

    public static readonly DependencyProperty CardHeightProperty =
        DependencyProperty.Register(
            nameof(CardHeight),
            typeof(double),
            typeof(TiltCard),
            new(210.0));

    public static readonly RoutedEvent CardClickEvent =
        EventManager.RegisterRoutedEvent(
            nameof(CardClick),
            RoutingStrategy.Bubble,
            typeof(RoutedEventHandler),
            typeof(TiltCard));

    public static readonly RoutedEvent CardDoubleClickEvent =
        EventManager.RegisterRoutedEvent(
            nameof(CardDoubleClick),
            RoutingStrategy.Bubble,
            typeof(RoutedEventHandler),
            typeof(TiltCard));

    private double _curX, _curY, _curScale = 1.0;
    private bool _hovered;
    private double _targetX, _targetY, _targetScale = 1.0;
    private int _mouseDownFrameCounter = 0;
    private DateTime _lastClickTime = DateTime.MinValue;
    private static readonly TimeSpan DoubleClickThreshold = TimeSpan.FromMilliseconds(300);

    public TiltCard()
    {
        InitializeComponent();
        Loaded += (_, __) =>
        {
            UpdateMeshToVisualSize();
            UpdateCameraDistance();
            CompositionTarget.Rendering += OnRenderFrame;
        };

        Vp.SizeChanged += (_, __) =>
        {
            UpdateMeshToVisualSize();
            UpdateCameraDistance();
        };

        Unloaded += (_, __) => { CompositionTarget.Rendering -= OnRenderFrame; };
    }

    public event RoutedEventHandler CardClick
    {
        add => AddHandler(CardClickEvent, value);
        remove => RemoveHandler(CardClickEvent, value);
    }

    public event RoutedEventHandler CardDoubleClick
    {
        add => AddHandler(CardDoubleClickEvent, value);
        remove => RemoveHandler(CardDoubleClickEvent, value);
    }

    public object? CardContent
    {
        get => GetValue(CardContentProperty);
        set => SetValue(CardContentProperty, value);
    }

    public double MaxAngle
    {
        get => (double)GetValue(MaxAngleProperty);
        set => SetValue(MaxAngleProperty, value);
    }

    public double HoverScale
    {
        get => (double)GetValue(HoverScaleProperty);
        set => SetValue(HoverScaleProperty, value);
    }

    public double Smooth
    {
        get => (double)GetValue(SmoothProperty);
        set => SetValue(SmoothProperty, value);
    }

    public double CardWidth
    {
        get => (double)GetValue(CardWidthProperty);
        set => SetValue(CardWidthProperty, value);
    }

    public double CardHeight
    {
        get => (double)GetValue(CardHeightProperty);
        set => SetValue(CardHeightProperty, value);
    }

    private void UpdateMeshToVisualSize()
    {
        var w = CardVisual.ActualWidth;
        var h = CardVisual.ActualHeight;
        if (w <= 0 || h <= 0)
        {
            w = CardWidth;
            h = CardHeight;

            if (w <= 0 || h <= 0)
                return;
        }

        var hw = w / 2.0;
        var hh = h / 2.0;

        CardMesh.Positions =
        [
            new(-hw, -hh, 0),
            new(hw, -hh, 0),
            new(hw, hh, 0),
            new(-hw, hh, 0)
        ];
    }

    private void CardVisual_OnMouseEnter(object sender, MouseEventArgs e)
    {
        _hovered = true;
        _targetScale = HoverScale;
    }

    private void CardVisual_OnMouseLeave(object sender, MouseEventArgs e)
    {
        _hovered = false;
        _targetX = 0;
        _targetY = 0;
        _targetScale = 1.0;
    }

    private void CardVisual_OnMouseMove(object sender, MouseEventArgs e)
    {
        ChangeAngles(e);
    }

    private void ChangeAngles(MouseEventArgs e)
    {
        if (!_hovered) 
            return;

        var p = e.GetPosition(CardVisual);
        var w = CardVisual.ActualWidth;
        var h = CardVisual.ActualHeight;

        if (w <= 0 || h <= 0) 
            return;


        var nx = p.X / w * 2.0 - 1.0; // -1..+1
        var ny = p.Y / h * 2.0 - 1.0;

        if (_mouseDownFrameCounter > 0)
        {
            nx *= 1.5;
            ny *= 1.5;
        }

        _targetX = -ny * MaxAngle; // rotateX
        _targetY = -nx * MaxAngle; // rotateY
    }

    private void OnRenderFrame(object? sender, EventArgs e)
    {
        var s = Math.Max(0.01, Math.Min(0.5, Smooth));

        _curX += (_targetX - _curX) * s;
        _curY += (_targetY - _curY) * s;
        _curScale += (_targetScale - _curScale) * s;

        RotX.Angle = _curX;
        RotY.Angle = _curY;

        Scale3D.ScaleX = _curScale;
        Scale3D.ScaleY = _curScale;
        Scale3D.ScaleZ = 1.0;

        if (Mouse.LeftButton == MouseButtonState.Released || _mouseDownFrameCounter > 2)
            _mouseDownFrameCounter--;
    }

    private void UpdateCameraDistance()
    {
        var w = Vp.ActualWidth;
        var h = Vp.ActualHeight;

        if (w <= 0) 
            w = CardWidth;

        if (h <= 0) 
            h = CardHeight;

        if (w <= 0 || h <= 0) 
            return;

        var fovH = Camera.FieldOfView * Math.PI / 180.0;
        var aspect = w / h;

        var fovV = 2.0 * Math.Atan(Math.Tan(fovH / 2.0) / aspect);

        var zForWidth = (w / 2.0) / Math.Tan(fovH / 2.0);
        var zForHeight = (h / 2.0) / Math.Tan(fovV / 2.0);

        var z = Math.Max(zForWidth, zForHeight);

        Camera.Position = new(0, 0, z);
    }

    private void CardVisual_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        _mouseDownFrameCounter = 15;
        ChangeAngles(e);
        
        var now = DateTime.Now;
        if (now - _lastClickTime <= DoubleClickThreshold)
        {
            _lastClickTime = DateTime.MinValue;
            RaiseEvent(new(CardDoubleClickEvent, this));
        }
        else
        {
            _lastClickTime = now;
            RaiseEvent(new(CardClickEvent, this));
        }
    }
}