using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using ImoutoViewer.Extensions;
using Microsoft.Xaml.Behaviors;

namespace ImoutoViewer.Behavior;

internal class FrameworkElementCustomToolTipBehavior : Behavior<FrameworkElement>
{
    #region Fields

    private const int MinToolTipSize = 150;
    private static FrameworkElement _currentTarget;
    private bool _sizeChangeSubscribed;
    private FrameworkElement _toolTipBorder;
    private bool _isOpened;

    #endregion Fields

    #region DependencyProperty

    public ObservableCollection<Inline> InlineList
    {
        get { return (ObservableCollection<Inline>) GetValue(InlineListProperty); }
        set { SetValue(InlineListProperty, value); }
    }

    public static readonly DependencyProperty InlineListProperty =
        DependencyProperty.Register("InlineList", typeof (ObservableCollection<Inline>),
            typeof (FrameworkElementCustomToolTipBehavior), null);

    #endregion DependencyProperty

    #region Methods

    private static void GetToolTipPosition(Size desiredTooltipSize,
        double bottomSpace,
        double rightSpace,
        Size targetSize,
        double leftSpace,
        double topSpace,
        out HorisontalPosition horisontalPosition,
        out VerticalPosition verticalPosition)
    {
        if (desiredTooltipSize.Height <= bottomSpace)
        {
            verticalPosition = VerticalPosition.Bottom;

            if (rightSpace + targetSize.Width >= desiredTooltipSize.Width)
            {
                horisontalPosition = HorisontalPosition.ToRight;
            }
            else if (leftSpace + targetSize.Width >= desiredTooltipSize.Width)
            {
                horisontalPosition = HorisontalPosition.ToLeft;
            }
            else if (rightSpace + leftSpace + targetSize.Width >= desiredTooltipSize.Width)
            {
                horisontalPosition = HorisontalPosition.Anywhere;
            }
            else
            {
                horisontalPosition = HorisontalPosition.Nowhere;
            }
        }
        else if (desiredTooltipSize.Height <= topSpace)
        {
            verticalPosition = VerticalPosition.Top;

            if (rightSpace + targetSize.Width >= desiredTooltipSize.Width)
            {
                horisontalPosition = HorisontalPosition.ToRight;
            }
            else if (leftSpace + targetSize.Width >= desiredTooltipSize.Width)
            {
                horisontalPosition = HorisontalPosition.ToLeft;
            }
            else if (rightSpace + leftSpace + targetSize.Width >= desiredTooltipSize.Width)
            {
                horisontalPosition = HorisontalPosition.Anywhere;
            }
            else
            {
                horisontalPosition = HorisontalPosition.Nowhere;
            }
        }
        else if (desiredTooltipSize.Width <= rightSpace)
        {
            horisontalPosition = HorisontalPosition.Rigth;

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
            horisontalPosition = HorisontalPosition.Left;

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
            horisontalPosition = HorisontalPosition.Nowhere;
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

    private void PlaceToolTip(UIElement target)
    {
        var mainWindow = UiHelper.FindVisualParent<Window>(target);
        var area = (UIElement) mainWindow.FindName("GridParent");

        if (!_sizeChangeSubscribed)
        {
            (area as FrameworkElement).SizeChanged += FrameworkElementCustomToolTipBehavior_SizeChanged;
            _sizeChangeSubscribed = true;
        }


        _toolTipBorder = (FrameworkElement) mainWindow.FindName("ToolTipBorder");
        var toolTipTextBlock = (TextBlock) mainWindow.FindName("ToolTipTextBlock");

        var targetPosition = target.TransformToAncestor(area).Transform(new Point(0, 0));
        var targetSize = target.RenderSize;
        var areaSize = area.RenderSize;

        var bottomSpace = areaSize.Height - targetPosition.Y - targetSize.Height;
        var topSpace = targetPosition.Y;
        var leftSpace = targetPosition.X;
        var rightSpace = areaSize.Width - targetPosition.X - targetSize.Width;

        var toolTipWidth = (targetSize.Width < MinToolTipSize) ? MinToolTipSize : targetSize.Width;
        var desiredTooltipSize = GetTextBlockSize(toolTipWidth);

        if (Math.Abs(desiredTooltipSize.Width - toolTipWidth) > 0.001)
        {
            desiredTooltipSize.Width = toolTipWidth + 10;
        }

        HorisontalPosition horisontalPosition;
        VerticalPosition verticalPosition;

        GetToolTipPosition(desiredTooltipSize, bottomSpace, rightSpace, targetSize, leftSpace, topSpace,
            out horisontalPosition, out verticalPosition);

        if (horisontalPosition == HorisontalPosition.Nowhere || verticalPosition == VerticalPosition.Nowhere)
        {
            return;
        }

        var tooltipPosition = new Point();
        var horisontalAlignment = HorizontalAlignment.Left;
        var verticalAlignment = VerticalAlignment.Top;

        switch (verticalPosition)
        {
            case VerticalPosition.Bottom:
                tooltipPosition.Y = targetPosition.Y + targetSize.Height;
                break;
            case VerticalPosition.Top:
                tooltipPosition.Y = targetPosition.Y - desiredTooltipSize.Height;
                break;
            case VerticalPosition.ToBottom:
                tooltipPosition.Y = targetPosition.Y;
                break;
            case VerticalPosition.ToTop:
                tooltipPosition.Y = targetPosition.Y + targetSize.Height - desiredTooltipSize.Height;
                break;
            case VerticalPosition.Anywhere:
                tooltipPosition.Y = areaSize.Height - desiredTooltipSize.Height;
                break;
        }

        switch (horisontalPosition)
        {
            case HorisontalPosition.Rigth:
                tooltipPosition.X = targetPosition.X + targetSize.Width;
                break;
            case HorisontalPosition.Left:
                tooltipPosition.X = targetPosition.X - desiredTooltipSize.Width;
                break;
            case HorisontalPosition.ToRight:
                tooltipPosition.X = targetPosition.X;
                break;
            case HorisontalPosition.ToLeft:
                tooltipPosition.X = targetPosition.X + targetSize.Width - desiredTooltipSize.Width;
                break;
            case HorisontalPosition.Anywhere:
                tooltipPosition.X = areaSize.Width - desiredTooltipSize.Width;
                break;
        }

        _toolTipBorder.Visibility = Visibility.Collapsed;

        _toolTipBorder.Width = toolTipWidth;
        _toolTipBorder.HorizontalAlignment = horisontalAlignment;
        _toolTipBorder.VerticalAlignment = verticalAlignment;

        _toolTipBorder.Margin = new Thickness(tooltipPosition.X, tooltipPosition.Y, 5, 5);
        toolTipTextBlock.Inlines.Clear();
        toolTipTextBlock.Inlines.AddRange(InlineList);

        _toolTipBorder.Visibility = Visibility.Visible;


        _isOpened = true;
    }

    #endregion Methods

    #region Event handlers

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
        _currentTarget.Unloaded += (o, args) => { HideToolTip(); };
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

    #endregion Event handlers

    #region Types

    private enum VerticalPosition
    {
        ToTop,
        Top,
        Bottom,
        ToBottom,
        Anywhere,
        Nowhere
    }

    private enum HorisontalPosition
    {
        ToLeft,
        Left,
        Rigth,
        ToRight,
        Anywhere,
        Nowhere
    }

    #endregion Types
}