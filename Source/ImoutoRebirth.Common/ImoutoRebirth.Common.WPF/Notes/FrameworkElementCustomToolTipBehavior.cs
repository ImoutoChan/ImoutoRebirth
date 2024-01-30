using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using Microsoft.Xaml.Behaviors;

namespace ImoutoRebirth.Common.WPF.Notes;

public class FrameworkElementCustomToolTipBehavior : Behavior<FrameworkElement>
{
    private const int MinToolTipSize = 150;
    private static FrameworkElement? _currentTarget;

    public static readonly DependencyProperty InlineListProperty =
        DependencyProperty.Register(
            nameof(InlineList),
            typeof(ObservableCollection<Inline>),
            typeof(FrameworkElementCustomToolTipBehavior),
            null);

    private bool _isOpened;
    private bool _sizeChangeSubscribed;
    private FrameworkElement? _toolTipBorder;

    public ObservableCollection<Inline> InlineList
    {
        get => (ObservableCollection<Inline>)GetValue(InlineListProperty);
        set => SetValue(InlineListProperty, value);
    }

    private static void GetToolTipPosition(
        Size desiredTooltipSize,
        double bottomSpace,
        double rightSpace,
        Size targetSize,
        double leftSpace,
        double topSpace,
        out HorizontalPosition horizontalPosition,
        out VerticalPosition verticalPosition)
    {
        if (desiredTooltipSize.Height <= bottomSpace)
        {
            verticalPosition = VerticalPosition.Bottom;

            if (rightSpace + targetSize.Width >= desiredTooltipSize.Width)
            {
                horizontalPosition = HorizontalPosition.ToRight;
            }
            else if (leftSpace + targetSize.Width >= desiredTooltipSize.Width)
            {
                horizontalPosition = HorizontalPosition.ToLeft;
            }
            else if (rightSpace + leftSpace + targetSize.Width >= desiredTooltipSize.Width)
            {
                horizontalPosition = HorizontalPosition.Anywhere;
            }
            else
            {
                horizontalPosition = HorizontalPosition.Nowhere;
            }
        }
        else if (desiredTooltipSize.Height <= topSpace)
        {
            verticalPosition = VerticalPosition.Top;

            if (rightSpace + targetSize.Width >= desiredTooltipSize.Width)
            {
                horizontalPosition = HorizontalPosition.ToRight;
            }
            else if (leftSpace + targetSize.Width >= desiredTooltipSize.Width)
            {
                horizontalPosition = HorizontalPosition.ToLeft;
            }
            else if (rightSpace + leftSpace + targetSize.Width >= desiredTooltipSize.Width)
            {
                horizontalPosition = HorizontalPosition.Anywhere;
            }
            else
            {
                horizontalPosition = HorizontalPosition.Nowhere;
            }
        }
        else if (desiredTooltipSize.Width <= rightSpace)
        {
            horizontalPosition = HorizontalPosition.Right;

            if (bottomSpace + targetSize.Height >= desiredTooltipSize.Height)
            {
                verticalPosition = VerticalPosition.ToBottom;
            }
            else if (topSpace + targetSize.Height >= desiredTooltipSize.Height)
            {
                verticalPosition = VerticalPosition.ToTop;
            }
            else if (topSpace + bottomSpace + targetSize.Height >= desiredTooltipSize.Height)
            {
                verticalPosition = VerticalPosition.Anywhere;
            }
            else
            {
                verticalPosition = VerticalPosition.Nowhere;
            }
        }
        else if (desiredTooltipSize.Width <= leftSpace)
        {
            horizontalPosition = HorizontalPosition.Left;

            if (bottomSpace + targetSize.Height >= desiredTooltipSize.Height)
            {
                verticalPosition = VerticalPosition.ToBottom;
            }
            else if (topSpace + targetSize.Height >= desiredTooltipSize.Height)
            {
                verticalPosition = VerticalPosition.ToTop;
            }
            else if (topSpace + bottomSpace + targetSize.Height >= desiredTooltipSize.Height)
            {
                verticalPosition = VerticalPosition.Anywhere;
            }
            else
            {
                verticalPosition = VerticalPosition.Nowhere;
            }
        }
        else
        {
            horizontalPosition = HorizontalPosition.Nowhere;
            verticalPosition = VerticalPosition.Nowhere;
        }
    }

    private Size GetTextBlockSize(double toolTipWidth)
    {
        var textBlock = new TextBlock();
        textBlock.Inlines.Clear();
        textBlock.Inlines.AddRange(InlineList.ToList());
        textBlock.TextWrapping = TextWrapping.Wrap;
        textBlock.Margin = new Thickness(5);

        textBlock.Measure(new Size(toolTipWidth, double.PositiveInfinity));
        textBlock.Arrange(new Rect(textBlock.DesiredSize));

        return new Size(textBlock.ActualWidth + 10, textBlock.ActualHeight + 10);
    }

    private void HideToolTip()
    {
        if (_toolTipBorder != null)
        {
            _toolTipBorder.Visibility = Visibility.Collapsed;
        }

        _isOpened = false;
    }

    private void PlaceToolTip(UIElement? target)
    {
        if (target == null)
            return;

        var mainWindow = UIHelper.FindVisualParent<Window>(target);
        if (mainWindow == null)
            return;

        var area = (UIElement?)UIHelper.FindVisualChild<Grid>(mainWindow, "GridParent");
        if (area == null)
            return;

        if (!_sizeChangeSubscribed)
        {
            ((FrameworkElement)area).SizeChanged += FrameworkElementCustomToolTipBehavior_SizeChanged;
            _sizeChangeSubscribed = true;
        }

        _toolTipBorder = UIHelper.FindVisualChild<FrameworkElement>(mainWindow, "ToolTipBorder");
        var toolTipTextBlock = UIHelper.FindVisualChild<TextBlock>(mainWindow, "ToolTipTextBlock");

        var targetPosition = target.TransformToAncestor(area).Transform(new Point(0, 0));
        var targetSize = target.RenderSize;
        var areaSize = area.RenderSize;

        var bottomSpace = areaSize.Height - targetPosition.Y - targetSize.Height;
        var topSpace = targetPosition.Y;
        var leftSpace = targetPosition.X;
        var rightSpace = areaSize.Width - targetPosition.X - targetSize.Width;

        var toolTipWidth = targetSize.Width < MinToolTipSize ? MinToolTipSize : targetSize.Width;
        var desiredTooltipSize = GetTextBlockSize(toolTipWidth);

        if (Math.Abs(desiredTooltipSize.Width - toolTipWidth) > 0.001)
        {
            desiredTooltipSize.Width = toolTipWidth + 10;
        }

        GetToolTipPosition(
            desiredTooltipSize,
            bottomSpace,
            rightSpace,
            targetSize,
            leftSpace,
            topSpace,
            out var horizontalPosition,
            out var verticalPosition);

        if (horizontalPosition == HorizontalPosition.Nowhere || verticalPosition == VerticalPosition.Nowhere)
        {
            return;
        }

        var tooltipPosition = new Point();
        var horizontalAlignment = HorizontalAlignment.Left;
        var verticalAlignment = VerticalAlignment.Top;

        tooltipPosition.Y = verticalPosition switch
        {
            VerticalPosition.Bottom => targetPosition.Y + targetSize.Height,
            VerticalPosition.Top => targetPosition.Y - desiredTooltipSize.Height,
            VerticalPosition.ToBottom => targetPosition.Y,
            VerticalPosition.ToTop => targetPosition.Y + targetSize.Height - desiredTooltipSize.Height,
            VerticalPosition.Anywhere => areaSize.Height - desiredTooltipSize.Height,
            _ => tooltipPosition.Y
        };

        tooltipPosition.X = horizontalPosition switch
        {
            HorizontalPosition.Right => targetPosition.X + targetSize.Width,
            HorizontalPosition.Left => targetPosition.X - desiredTooltipSize.Width,
            HorizontalPosition.ToRight => targetPosition.X,
            HorizontalPosition.ToLeft => targetPosition.X + targetSize.Width - desiredTooltipSize.Width,
            HorizontalPosition.Anywhere => areaSize.Width - desiredTooltipSize.Width,
            _ => tooltipPosition.X
        };

        if (_toolTipBorder == null || toolTipTextBlock == null)
            return;

        _toolTipBorder.Visibility = Visibility.Collapsed;

        _toolTipBorder.Width = toolTipWidth;
        _toolTipBorder.HorizontalAlignment = horizontalAlignment;
        _toolTipBorder.VerticalAlignment = verticalAlignment;

        _toolTipBorder.Margin = new Thickness(tooltipPosition.X, tooltipPosition.Y, 5, 5);
        toolTipTextBlock.Inlines.Clear();
        toolTipTextBlock.Inlines.AddRange(InlineList);

        _toolTipBorder.Visibility = Visibility.Visible;

        _isOpened = true;
    }

    protected override void OnAttached()
    {
        base.OnAttached();
        AssociatedObject.MouseLeave += AssociatedObject_MouseLeave;
        AssociatedObject.MouseEnter += AssociatedObject_MouseEnter;
    }

    protected override void OnDetaching()
    {
        base.OnDetaching();
        AssociatedObject.MouseLeave -= AssociatedObject_MouseLeave;
        AssociatedObject.MouseEnter -= AssociatedObject_MouseEnter;
    }

    private void AssociatedObject_MouseEnter(object sender, MouseEventArgs e)
    {
        _currentTarget = sender as FrameworkElement;
        PlaceToolTip(_currentTarget);

        if (_currentTarget == null)
            return;

        _currentTarget.Unloaded += (_, _) => HideToolTip();
    }

    private void AssociatedObject_MouseLeave(object sender, MouseEventArgs e)
    {
        HideToolTip();
    }

    private void FrameworkElementCustomToolTipBehavior_SizeChanged(object sender, SizeChangedEventArgs e)
    {
        if (_isOpened)
        {
            PlaceToolTip(_currentTarget);
        }
    }

    private enum VerticalPosition
    {
        ToTop,
        Top,
        Bottom,
        ToBottom,
        Anywhere,
        Nowhere
    }

    private enum HorizontalPosition
    {
        ToLeft,
        Left,
        Right,
        ToRight,
        Anywhere,
        Nowhere
    }
}
